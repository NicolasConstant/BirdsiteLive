using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
                    It.IsAny<DateTime>(),
                    It.Is<string>(y => y == null),
                    It.Is<string>(y => y == null),
                    It.Is<bool>(y => y == false)
                ))
                .Returns(Task.CompletedTask);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ProcessAsync_Exception_Test()
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
                Tweets = new[]
                {
                    tweet1,
                    tweet2
                },
                Followers = new[]
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
                    It.IsAny<DateTime>(),
                    It.Is<string>(y => y == null),
                    It.Is<string>(y => y == null),
                    It.Is<bool>(y => y == false)
                ))
                .Throws(new ArgumentException());

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
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
                    It.IsAny<DateTime>(),
                    It.Is<string>(y => y == null),
                    It.Is<string>(y => y == null),
                    It.Is<bool>(y => y == false)
                ))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
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
                    It.IsAny<DateTime>(),
                    It.Is<string>(y => y == null),
                    It.Is<string>(y => y == null),
                    It.Is<bool>(y => y == false)
                ))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_NoTweets_Test()
        {
            #region Stubs
            var user = new SyncTwitterUser
            {
                Id = 1,
                LastTweetPostedId = 42, 
                LastSync = DateTime.UtcNow.AddDays(-3)
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
                Tweets = Array.Empty<ExtractedTweet>(),
                Followers = new[]
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
                    It.Is<SyncTwitterUser>(y => y.LastTweetPostedId == 42 
                                                && y.LastSync > DateTime.UtcNow.AddDays(-1))
                ))
                .Returns(Task.CompletedTask);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_NoFollower_Test()
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

            var usersWithTweets = new UserWithDataToSync
            {
                Tweets = new[]
                {
                    tweet1,
                    tweet2
                },
                Followers = Array.Empty<Follower>(),
                User = user
            };

            var loggerMock = new Mock<ILogger<SaveProgressionProcessor>>();
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            removeTwitterAccountActionMock
                .Setup(x => x.ProcessAsync(It.Is<SyncTwitterUser>(y => y.Id == user.Id)))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new SaveProgressionProcessor(twitterUserDalMock.Object, loggerMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(usersWithTweets, CancellationToken.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            loggerMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

    }
}