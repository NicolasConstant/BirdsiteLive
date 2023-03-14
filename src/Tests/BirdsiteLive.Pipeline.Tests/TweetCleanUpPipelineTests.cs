using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts.Federation;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests
{
    [TestClass]
    public class TweetCleanUpPipelineTests
    {
        [TestMethod]
        public async Task Process()
        {
            #region Stubs
            var ct = new CancellationTokenSource(10);
            #endregion

            #region Mocks
            var retrieveTweetsToDeleteProcessor = new Mock<IRetrieveTweetsToDeleteProcessor>(MockBehavior.Strict);
            retrieveTweetsToDeleteProcessor
                .Setup(x => x.GetTweetsAsync(
                    It.IsAny<BufferBlock<TweetToDelete>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.Delay(0));

            var deleteTweetsProcessor = new Mock<IDeleteTweetsProcessor>(MockBehavior.Strict);
            var saveDeletedTweetStatusProcessor = new Mock<ISaveDeletedTweetStatusProcessor>(MockBehavior.Strict);
            var logger = new Mock<ILogger<TweetCleanUpPipeline>>();

            #endregion

            var pipeline = new TweetCleanUpPipeline(retrieveTweetsToDeleteProcessor.Object, deleteTweetsProcessor.Object, saveDeletedTweetStatusProcessor.Object, logger.Object);
            await pipeline.ExecuteAsync(ct.Token);

            #region Validations
            retrieveTweetsToDeleteProcessor.VerifyAll();
            deleteTweetsProcessor.VerifyAll();
            saveDeletedTweetStatusProcessor.VerifyAll();
            #endregion
        }
    }
}