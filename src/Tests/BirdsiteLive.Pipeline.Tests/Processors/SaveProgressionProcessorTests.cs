using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Twitter.Models;
using Castle.DynamicProxy.Contributors;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors
{
    [TestClass]
    public class SaveProgressionProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_Test()
        {
            #region Stubs
            var user = new SyncTwitterUser
            {
                Id = 1
            };
            var tweet1 = new ExtractedTweet
            {
                Id = 36
            };
            var tweet2 = new ExtractedTweet
            {
                Id = 37
            };
            var follower1 = new Follower
            {
                FollowingsSyncStatus = new Dictionary<int, long>
                {
                    {1, 37}
                }
            };

            var usersWithTweets = new UserWithDataToSync
            {
                Tweets = new []
                {
                    tweet1,
                    tweet2
                },
                Followers = new []
                {
                    follower1
                },
                User = user
            };

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.UpdateTwitterUserAsync(
                    It.Is<int>(y => y == user.Id),
                    It.Is<long>(y => y == tweet2.Id),
                    It.Is<long>(y => y == tweet2.Id),
                    It.Is<int>(y => y == 0),
                    It.IsAny<DateTime>()
                ))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_PartiallySynchronized_Test()
        {
            #region Stubs
            var user = new SyncTwitterUser
            {
                Id = 1
            };
            var tweet1 = new ExtractedTweet
            {
                Id = 36
            };
            var tweet2 = new ExtractedTweet
            {
                Id = 37
            };
            var tweet3 = new ExtractedTweet
            {
                Id = 38
            };
            var follower1 = new Follower
            {
                FollowingsSyncStatus = new Dictionary<int, long>
                {
                    {1, 37}
                }
            };

            var usersWithTweets = new UserWithDataToSync
            {
                Tweets = new[]
                {
                    tweet1,
                    tweet2,
                    tweet3
                },
                Followers = new[]
                {
                    follower1
                },
                User = user
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.UpdateTwitterUserAsync(
                    It.Is<int>(y => y == user.Id),
                    It.Is<long>(y => y == tweet3.Id),
                    It.Is<long>(y => y == tweet2.Id),
                    It.Is<int>(y => y == 0),
                    It.IsAny<DateTime>()
                ))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_PartiallySynchronized_MultiUsers_Test()
        {
            #region Stubs
            var user = new SyncTwitterUser
            {
                Id = 1
            };
            var tweet1 = new ExtractedTweet
            {
                Id = 36
            };
            var tweet2 = new ExtractedTweet
            {
                Id = 37
            };
            var tweet3 = new ExtractedTweet
            {
                Id = 38
            };
            var follower1 = new Follower
            {
                FollowingsSyncStatus = new Dictionary<int, long>
                {
                    {1, 37}
                }
            };
            var follower2 = new Follower
            {
                FollowingsSyncStatus = new Dictionary<int, long>
                {
                    {1, 38}
                }
            };

            var usersWithTweets = new UserWithDataToSync
            {
                Tweets = new[]
                {
                    tweet1,
                    tweet2,
                    tweet3
                },
                Followers = new[]
                {
                    follower1,
                    follower2
                },
                User = user
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.UpdateTwitterUserAsync(
                    It.Is<int>(y => y == user.Id),
                    It.Is<long>(y => y == tweet3.Id),
                    It.Is<long>(y => y == tweet2.Id),
                    It.Is<int>(y => y == 0),
                    It.IsAny<DateTime>()
                ))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            #endregion
        }
    }
}