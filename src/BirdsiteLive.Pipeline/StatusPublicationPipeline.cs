using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline
{
    public interface IStatusPublicationPipeline
    {
        Task ExecuteAsync(CancellationToken ct);
    }

    public class StatusPublicationPipeline : IStatusPublicationPipeline
    {
        private readonly IRetrieveTwitterUsersProcessor _retrieveTwitterAccountsProcessor;
        private readonly IRetrieveTweetsProcessor _retrieveTweetsProcessor;
        private readonly IRetrieveFollowersProcessor _retrieveFollowersProcessor;
        private readonly ISendTweetsToFollowersProcessor _sendTweetsToFollowersProcessor;
        
        #region Ctor
        public StatusPublicationPipeline(IRetrieveTweetsProcessor retrieveTweetsProcessor, IRetrieveTwitterUsersProcessor retrieveTwitterAccountsProcessor, IRetrieveFollowersProcessor retrieveFollowersProcessor, ISendTweetsToFollowersProcessor sendTweetsToFollowersProcessor)
        {
            _retrieveTweetsProcessor = retrieveTweetsProcessor;
            _retrieveTwitterAccountsProcessor = retrieveTwitterAccountsProcessor;
            _retrieveFollowersProcessor = retrieveFollowersProcessor;
            _sendTweetsToFollowersProcessor = sendTweetsToFollowersProcessor;
        }
        #endregion

        public async Task ExecuteAsync(CancellationToken ct)
        {
            // Create blocks 
            var twitterUsersBufferBlock = new BufferBlock<SyncTwitterUser[]>(new DataflowBlockOptions { BoundedCapacity = 1, CancellationToken = ct});
            var retrieveTweetsBlock = new TransformBlock<SyncTwitterUser[], UserWithTweetsToSync[]>(async x => await _retrieveTweetsProcessor.ProcessAsync(x, ct));
            var retrieveTweetsBufferBlock = new BufferBlock<UserWithTweetsToSync[]>(new DataflowBlockOptions { BoundedCapacity = 1, CancellationToken = ct });
            var retrieveFollowersBlock = new TransformManyBlock<UserWithTweetsToSync[], UserWithTweetsToSync>(async x => await _retrieveFollowersProcessor.ProcessAsync(x, ct));
            var retrieveFollowersBufferBlock = new BufferBlock<UserWithTweetsToSync>(new DataflowBlockOptions { BoundedCapacity = 20, CancellationToken = ct });
            var sendTweetsToFollowersBlock = new ActionBlock<UserWithTweetsToSync>(async x => await _sendTweetsToFollowersProcessor.ProcessAsync(x, ct), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5, CancellationToken = ct});

            // Link pipeline
            twitterUsersBufferBlock.LinkTo(retrieveTweetsBlock, new DataflowLinkOptions {PropagateCompletion = true});
            retrieveTweetsBlock.LinkTo(retrieveTweetsBufferBlock, new DataflowLinkOptions { PropagateCompletion = true });
            retrieveTweetsBufferBlock.LinkTo(retrieveFollowersBlock, new DataflowLinkOptions { PropagateCompletion = true });
            retrieveFollowersBlock.LinkTo(retrieveFollowersBufferBlock, new DataflowLinkOptions { PropagateCompletion = true });
            retrieveFollowersBufferBlock.LinkTo(sendTweetsToFollowersBlock, new DataflowLinkOptions { PropagateCompletion = true });

            // Launch twitter user retriever
            var retrieveTwitterAccountsTask = _retrieveTwitterAccountsProcessor.GetTwitterUsersAsync(twitterUsersBufferBlock, ct);

            // Wait
            await Task.WhenAny(new []{ retrieveTwitterAccountsTask , sendTweetsToFollowersBlock.Completion});

            var foreground = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("An error occured, pipeline stopped");
            Console.ForegroundColor = foreground;
        }
    }
}
