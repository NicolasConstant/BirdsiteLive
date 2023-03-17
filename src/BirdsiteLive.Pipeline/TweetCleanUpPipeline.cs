using System.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline
{
    public interface ITweetCleanUpPipeline
    {
        Task ExecuteAsync(CancellationToken ct);
    }

    public class TweetCleanUpPipeline : ITweetCleanUpPipeline
    {
        private readonly IRetrieveTweetsToDeleteProcessor _retrieveTweetsToDeleteProcessor;
        private readonly IDeleteTweetsProcessor _deleteTweetsProcessor;
        private readonly ISaveDeletedTweetStatusProcessor _saveDeletedTweetStatusProcessor;
        private readonly ILogger<TweetCleanUpPipeline> _logger;

        #region Ctor
        public TweetCleanUpPipeline(IRetrieveTweetsToDeleteProcessor retrieveTweetsToDeleteProcessor, IDeleteTweetsProcessor deleteTweetsProcessor, ISaveDeletedTweetStatusProcessor saveDeletedTweetStatusProcessor, ILogger<TweetCleanUpPipeline> logger)
        {
            _retrieveTweetsToDeleteProcessor = retrieveTweetsToDeleteProcessor;
            _deleteTweetsProcessor = deleteTweetsProcessor;
            _saveDeletedTweetStatusProcessor = saveDeletedTweetStatusProcessor;
            _logger = logger;
        }
        #endregion

        public async Task ExecuteAsync(CancellationToken ct)
        {
            // Create blocks 
            var tweetsToDeleteBufferBlock = new BufferBlock<TweetToDelete>(new DataflowBlockOptions
            {
                BoundedCapacity = 200,
                CancellationToken = ct
            });
            var deleteTweetsBlock = new TransformBlock<TweetToDelete, TweetToDelete>(
                async x => await _deleteTweetsProcessor.ProcessAsync(x, ct),
                new ExecutionDataflowBlockOptions()
                {
                    MaxDegreeOfParallelism = 5, 
                    CancellationToken = ct
                });
            var deletedTweetsBufferBlock = new BufferBlock<TweetToDelete>(new DataflowBlockOptions
            {
                BoundedCapacity = 200,
                CancellationToken = ct
            });
            var saveProgressionBlock = new ActionBlock<TweetToDelete>(
                async x => await _saveDeletedTweetStatusProcessor.ProcessAsync(x, ct),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 5, 
                    CancellationToken = ct
                });

            // Link pipeline
            var dataflowLinkOptions = new DataflowLinkOptions { PropagateCompletion = true };
            tweetsToDeleteBufferBlock.LinkTo(deleteTweetsBlock, dataflowLinkOptions);
            deleteTweetsBlock.LinkTo(deletedTweetsBufferBlock, dataflowLinkOptions);
            deletedTweetsBufferBlock.LinkTo(saveProgressionBlock, dataflowLinkOptions);

            // Launch tweet retriever 
            var retrieveTweetsToDeleteTask = _retrieveTweetsToDeleteProcessor.GetTweetsAsync(tweetsToDeleteBufferBlock, ct);

            // Wait 
            await Task.WhenAny(new[] { retrieveTweetsToDeleteTask, saveProgressionBlock.Completion });

            var ex = retrieveTweetsToDeleteTask.IsFaulted ? retrieveTweetsToDeleteTask.Exception : saveProgressionBlock.Completion.Exception;
            _logger.LogCritical(ex, "An error occurred, pipeline stopped");
        }
    }
}