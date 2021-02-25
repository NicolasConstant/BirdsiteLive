using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests
{
    [TestClass]
    public class StatusPublicationPipelineTests
    {
        [TestMethod]
        public async Task ExecuteAsync_Test()
        {
            #region Stubs
            var ct = new CancellationTokenSource(10);
            #endregion

            #region Mocks
            var retrieveTwitterUsersProcessor = new Mock<IRetrieveTwitterUsersProcessor>(MockBehavior.Strict);
            retrieveTwitterUsersProcessor
                .Setup(x => x.GetTwitterUsersAsync(
                    It.IsAny<BufferBlock<SyncTwitterUser[]>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.Delay(0));

            var retrieveTweetsProcessor = new Mock<IRetrieveTweetsProcessor>(MockBehavior.Strict);
            var retrieveFollowersProcessor = new Mock<IRetrieveFollowersProcessor>(MockBehavior.Strict);
            var sendTweetsToFollowersProcessor = new Mock<ISendTweetsToFollowersProcessor>(MockBehavior.Strict);
            var saveProgressionProcessor = new Mock<ISaveProgressionProcessor>(MockBehavior.Strict);
            var logger = new Mock<ILogger<StatusPublicationPipeline>>();
            #endregion

            var pipeline = new StatusPublicationPipeline(retrieveTweetsProcessor.Object, retrieveTwitterUsersProcessor.Object, retrieveFollowersProcessor.Object, sendTweetsToFollowersProcessor.Object, saveProgressionProcessor.Object, logger.Object);
            await pipeline.ExecuteAsync(ct.Token);

            #region Validations
            retrieveTwitterUsersProcessor.VerifyAll();
            retrieveTweetsProcessor.VerifyAll();
            retrieveFollowersProcessor.VerifyAll();
            sendTweetsToFollowersProcessor.VerifyAll();
            saveProgressionProcessor.VerifyAll();
            logger.VerifyAll();
            #endregion
        }
    }
}