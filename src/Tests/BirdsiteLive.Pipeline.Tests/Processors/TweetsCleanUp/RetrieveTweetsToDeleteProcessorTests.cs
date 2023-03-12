using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.TweetsCleanUp;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Org.BouncyCastle.Crypto;

namespace BirdsiteLive.Pipeline.Tests.Processors.TweetsCleanUp
{
    [TestClass]
    public class RetrieveTweetsToDeleteProcessorTests
    {
        [TestMethod]
        public async Task Process_Test()
        {
            #region Stubs
            var settings = new InstanceSettings
            {

            };
            var bufferBlock = new BufferBlock<TweetToDelete>();

            var tweetId1 = 42;
            var tweetId2 = 43;

            var tweet1 = new SyncTweet
            {
                Id = tweetId1
            };
            var tweet2 = new SyncTweet
            {
                Id = tweetId2
            };
            var batch = new List<SyncTweet>
            {
                tweet1,
                tweet2
            };
            #endregion

            #region Mocks
            var dalMock = new Mock<ISyncTweetsPostgresDal>(MockBehavior.Strict);
            dalMock
                .Setup(x => x.GetTweetsOlderThanAsync(It.IsAny<DateTime>(), It.Is<long>(y => y == -1), It.Is<int>(y => y == 100)))
                .ReturnsAsync(batch);
            #endregion

            var processor = new RetrieveTweetsToDeleteProcessor(dalMock.Object, settings);
            processor.GetTweetsAsync(bufferBlock, CancellationToken.None);
            
            await Task.Delay(TimeSpan.FromSeconds(1));

            #region Validations
            dalMock.VerifyAll();

            Assert.AreEqual(batch.Count, bufferBlock.Count);

            bufferBlock.TryReceiveAll(out var all);
            foreach (var tweet in all)
            {
                Assert.IsTrue(batch.Any(x => x.Id == tweet.Tweet.Id));
            }
            #endregion
        }
    }
}