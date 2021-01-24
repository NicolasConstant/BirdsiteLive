using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub.Models;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Processors.SubTasks;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors.SubTasks
{
    [TestClass]
    public class SendTweetsToSharedInboxTests
    {
        [TestMethod]
        public async Task ExecuteAsync_SingleTweet_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                }
            };

            var noteId = "noteId";
            var note = new Note()
            {
                id = noteId
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);
            activityPubService
                .Setup(x => x.PostNewNoteActivity(
                    It.Is<Note>(y => y.id == noteId),
                    It.Is<string>(y => y == twitterHandle),
                    It.Is<string>(y => y == tweetId.ToString()),
                    It.Is<string>(y => y == host),
                    It.Is<string>(y => y == inbox)))
                .Returns(Task.CompletedTask);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            statusServiceMock
                .Setup(x => x.GetStatus(
                It.Is<string>(y => y == twitterHandle),
                It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                .Returns(note);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleTweet_Reply_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                    IsReply = true,
                    IsThread = false
                }
            };

            var noteId = "noteId";
            var note = new Note()
            {
                id = noteId
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleTweet_ReplyThread_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                    IsReply = true,
                    IsThread = true
                }
            };

            var noteId = "noteId";
            var note = new Note()
            {
                id = noteId
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);
            activityPubService
                .Setup(x => x.PostNewNoteActivity(
                    It.Is<Note>(y => y.id == noteId),
                    It.Is<string>(y => y == twitterHandle),
                    It.Is<string>(y => y == tweetId.ToString()),
                    It.Is<string>(y => y == host),
                    It.Is<string>(y => y == inbox)))
                .Returns(Task.CompletedTask);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            statusServiceMock
                .Setup(x => x.GetStatus(
                It.Is<string>(y => y == twitterHandle),
                It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                .Returns(note);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleTweet_PublishReply_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                    IsReply = true,
                    IsThread = false
                }
            };

            var noteId = "noteId";
            var note = new Note()
            {
                id = noteId
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = true
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);
            activityPubService
                .Setup(x => x.PostNewNoteActivity(
                    It.Is<Note>(y => y.id == noteId),
                    It.Is<string>(y => y == twitterHandle),
                    It.Is<string>(y => y == tweetId.ToString()),
                    It.Is<string>(y => y == host),
                    It.Is<string>(y => y == inbox)))
                .Returns(Task.CompletedTask);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            statusServiceMock
                .Setup(x => x.GetStatus(
                It.Is<string>(y => y == twitterHandle),
                It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                .Returns(note);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_MultipleTweets_Test()
        {
            #region Stubs
            var tweetId1 = 10;
            var tweetId2 = 11;
            var tweetId3 = 12;
            var tweets = new List<ExtractedTweet>();
            foreach (var tweetId in new[] { tweetId1, tweetId2, tweetId3 })
            {
                tweets.Add(new ExtractedTweet
                {
                    Id = tweetId
                });
            }

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 10}}
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 8}}
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 7}}
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);
            foreach (var tweetId in new[] { tweetId2, tweetId3 })
            {
                activityPubService
                    .Setup(x => x.PostNewNoteActivity(
                        It.Is<Note>(y => y.id == tweetId.ToString()),
                        It.Is<string>(y => y == twitterHandle),
                        It.Is<string>(y => y == tweetId.ToString()),
                        It.Is<string>(y => y == host),
                        It.Is<string>(y => y == inbox)))
                    .Returns(Task.CompletedTask);
            }

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            foreach (var tweetId in new[] { tweetId2, tweetId3 })
            {
                statusServiceMock
                    .Setup(x => x.GetStatus(
                        It.Is<string>(y => y == twitterHandle),
                        It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                    .Returns(new Note { id = tweetId.ToString() });
            }

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId3)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task ExecuteAsync_MultipleTweets_Error_Test()
        {
            #region Stubs
            var tweetId1 = 10;
            var tweetId2 = 11;
            var tweetId3 = 12;
            var tweets = new List<ExtractedTweet>();
            foreach (var tweetId in new[] { tweetId1, tweetId2, tweetId3 })
            {
                tweets.Add(new ExtractedTweet
                {
                    Id = tweetId
                });
            }

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 10}}
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 8}}
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> {{twitterUserId, 7}}
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);

            activityPubService
                .Setup(x => x.PostNewNoteActivity(
                    It.Is<Note>(y => y.id == tweetId2.ToString()),
                    It.Is<string>(y => y == twitterHandle),
                    It.Is<string>(y => y == tweetId2.ToString()),
                    It.Is<string>(y => y == host),
                    It.Is<string>(y => y == inbox)))
                .Returns(Task.CompletedTask);

            activityPubService
                .Setup(x => x.PostNewNoteActivity(
                    It.Is<Note>(y => y.id == tweetId3.ToString()),
                    It.Is<string>(y => y == twitterHandle),
                    It.Is<string>(y => y == tweetId3.ToString()),
                    It.Is<string>(y => y == host),
                    It.Is<string>(y => y == inbox)))
                .Throws(new HttpRequestException());

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            foreach (var tweetId in new[] { tweetId2, tweetId3 })
            {
                statusServiceMock
                    .Setup(x => x.GetStatus(
                        It.Is<string>(y => y == twitterHandle),
                        It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                    .Returns(new Note { id = tweetId.ToString() });
            }

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId2)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);

            try
            {
                await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());
            }
            finally
            {
                #region Validations
                activityPubService.VerifyAll();
                statusServiceMock.VerifyAll();
                followersDalMock.VerifyAll();
                #endregion
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_SingleTweet_ParsingError_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                }
            };

            var noteId = "noteId";
            var note = new Note()
            {
                id = noteId
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            statusServiceMock
                .Setup(x => x.GetStatus(
                It.Is<string>(y => y == twitterHandle),
                It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                .Throws(new ArgumentException("Invalid pattern blabla at offset 9"));

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            foreach (var follower in followers)
            {
                followersDalMock
                    .Setup(x => x.UpdateFollowerAsync(
                        It.Is<Follower>(y => y.Id == follower.Id && y.FollowingsSyncStatus[twitterUserId] == tweetId)))
                    .Returns(Task.CompletedTask);
            }

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);
            await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            #region Validations
            activityPubService.VerifyAll();
            statusServiceMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task ExecuteAsync_SingleTweet_ArgumentException_Test()
        {
            #region Stubs
            var tweetId = 10;
            var tweets = new List<ExtractedTweet>
            {
                new ExtractedTweet
                {
                    Id = tweetId,
                }
            };

            var twitterHandle = "Test";
            var twitterUserId = 7;
            var twitterUser = new SyncTwitterUser
            {
                Id = twitterUserId,
                Acct = twitterHandle
            };

            var host = "domain.ext";
            var inbox = "/inbox";
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 1,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 9 } }
                },
                new Follower
                {
                    Id = 2,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 8 } }
                },
                new Follower
                {
                    Id = 3,
                    Host = host,
                    SharedInboxRoute = inbox,
                    FollowingsSyncStatus = new Dictionary<int, long> { { twitterUserId, 7 } }
                }
            };

            var settings = new InstanceSettings
            {
                PublishReplies = false
            };
            #endregion

            #region Mocks
            var activityPubService = new Mock<IActivityPubService>(MockBehavior.Strict);

            var statusServiceMock = new Mock<IStatusService>(MockBehavior.Strict);
            statusServiceMock
                .Setup(x => x.GetStatus(
                It.Is<string>(y => y == twitterHandle),
                It.Is<ExtractedTweet>(y => y.Id == tweetId)))
                .Throws(new ArgumentException());

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            var loggerMock = new Mock<ILogger<SendTweetsToSharedInboxTask>>();
            #endregion

            var task = new SendTweetsToSharedInboxTask(activityPubService.Object, statusServiceMock.Object, followersDalMock.Object, settings, loggerMock.Object);

            try
            {
                await task.ExecuteAsync(tweets.ToArray(), twitterUser, host, followers.ToArray());

            }
            finally
            {
                #region Validations
                activityPubService.VerifyAll();
                statusServiceMock.VerifyAll();
                followersDalMock.VerifyAll();
                #endregion
            }
        }
    }
}