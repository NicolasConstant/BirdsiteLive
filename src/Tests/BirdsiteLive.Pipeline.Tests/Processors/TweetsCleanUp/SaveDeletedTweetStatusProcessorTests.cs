using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.TweetsCleanUp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors.TweetsCleanUp
{
    [TestClass]
    public class SaveDeletedTweetStatusProcessorTests
    {
        [TestMethod]
        public async Task Process_DeleteSuccessfull_Test()
        {
            #region Stubs
            var settings = new InstanceSettings { };
            var tweetId = 42;
            var tweetToDelete = new TweetToDelete
            {
                DeleteSuccessful = true,
                Tweet = new SyncTweet
                {
                    Id = tweetId
                }
            };
            #endregion

            #region Mocks
            var dalMock = new Mock<ISyncTweetsPostgresDal>(MockBehavior.Strict);
            dalMock
                .Setup(x => x.DeleteTweetAsync(It.Is<long>(y => y == tweetId)))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new SaveDeletedTweetStatusProcessor(dalMock.Object, settings);
            await processor.ProcessAsync(tweetToDelete, CancellationToken.None);

            #region Validations
            dalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task Process_Expired_Test()
        {
            #region Stubs
            var settings = new InstanceSettings { };
            var tweetId = 42;
            var tweetToDelete = new TweetToDelete
            {
                DeleteSuccessful = false,
                Tweet = new SyncTweet
                {
                    Id = tweetId,
                    PublishedAt = DateTime.UtcNow.AddDays(-30)
                }
            };
            #endregion

            #region Mocks
            var dalMock = new Mock<ISyncTweetsPostgresDal>(MockBehavior.Strict);
            dalMock
                .Setup(x => x.DeleteTweetAsync(It.Is<long>(y => y == tweetId)))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new SaveDeletedTweetStatusProcessor(dalMock.Object, settings);
            await processor.ProcessAsync(tweetToDelete, CancellationToken.None);

            #region Validations
            dalMock.VerifyAll();
            #endregion
        }
    }
}