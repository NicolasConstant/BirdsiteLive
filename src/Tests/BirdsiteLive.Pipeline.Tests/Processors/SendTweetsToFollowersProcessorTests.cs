using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Pipeline.Processors.SubTasks;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors
{
    [TestClass]
    public class SendTweetsToFollowersProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_SameInstance_SharedInbox_OneTweet_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host = "domain.ext";
            var sharedInbox = "/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new []
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new []
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host,
                        SharedInboxRoute = sharedInbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host,
                        SharedInboxRoute = sharedInbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);
            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host),
                    It.Is<Follower[]>(y => y.Length == 2)))
                .Returns(Task.CompletedTask);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_SharedInbox_OneTweet_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var sharedInbox = "/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        SharedInboxRoute = sharedInbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        SharedInboxRoute = sharedInbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);
            foreach (var host in new [] { host1, host2})
            {
                sendTweetsToSharedInboxTaskMock
                    .Setup(x => x.ExecuteAsync(
                        It.Is<ExtractedTweet[]>(y => y.Length == 1),
                        It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                        It.Is<string>(y => y == host),
                        It.Is<Follower[]>(y => y.Length == 1)))
                    .Returns(Task.CompletedTask);
            }

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            
            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_SharedInbox_OneTweet_Error_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var sharedInbox = "/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        SharedInboxRoute = sharedInbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        SharedInboxRoute = sharedInbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);
            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host1),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Returns(Task.CompletedTask);

            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host2),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Throws(new Exception());

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            
            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 1)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_SharedInbox_OneTweet_ErrorReset_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var sharedInbox = "/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        SharedInboxRoute = sharedInbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        SharedInboxRoute = sharedInbox,
                        PostingErrorCount = 50
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);
            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host1),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Returns(Task.CompletedTask);

            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host2),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Returns(Task.CompletedTask);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 0)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_SharedInbox_OneTweet_ErrorAndReset_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var sharedInbox = "/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        SharedInboxRoute = sharedInbox,
                        PostingErrorCount = 50
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        SharedInboxRoute = sharedInbox,
                        PostingErrorCount = 50
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);
            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host1),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Returns(Task.CompletedTask);

            sendTweetsToSharedInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct),
                    It.Is<string>(y => y == host2),
                    It.Is<Follower[]>(y => y.Length == 1)))
                .Throws(new Exception());

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId1 && y.PostingErrorCount == 0)))
                .Returns(Task.CompletedTask);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 51)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_SameInstance_Inbox_OneTweet_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host = "domain.ext";
            var inbox = "/user/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host,
                        InboxRoute = inbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host,
                        InboxRoute = inbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);
            foreach (var userId in new[] { userId1, userId2 })
            {
                sendTweetsToInboxTaskMock
                    .Setup(x => x.ExecuteAsync(
                        It.Is<ExtractedTweet[]>(y => y.Length == 1),
                        It.Is<Follower>(y => y.Id == userId),
                        It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                    .Returns(Task.CompletedTask);
            }

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_Inbox_OneTweet_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var inbox = "/user/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        InboxRoute = inbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        InboxRoute = inbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);
            foreach (var userId in new[] { userId1, userId2 })
            {
                sendTweetsToInboxTaskMock
                    .Setup(x => x.ExecuteAsync(
                        It.Is<ExtractedTweet[]>(y => y.Length == 1),
                        It.Is<Follower>(y => y.Id == userId),
                        It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                    .Returns(Task.CompletedTask);
            }

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_Inbox_OneTweet_Error_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var inbox = "/user/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        InboxRoute = inbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        InboxRoute = inbox
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);
            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Returns(Task.CompletedTask);

            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId2),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Throws(new Exception());

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            
            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 1)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_Inbox_OneTweet_ErrorReset_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var inbox = "/user/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        InboxRoute = inbox
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        InboxRoute = inbox,
                        PostingErrorCount = 50
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);
            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Returns(Task.CompletedTask);

            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId2),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Returns(Task.CompletedTask);

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 0)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_MultiInstances_Inbox_OneTweet_ErrorAndReset_Test()
        {
            #region Stubs
            var tweetId = 1;
            var host1 = "domain1.ext";
            var host2 = "domain2.ext";
            var inbox = "/user/inbox";
            var userId1 = 2;
            var userId2 = 3;
            var userAcct = "user";

            var userWithTweets = new UserWithDataToSync()
            {
                Tweets = new[]
                {
                    new ExtractedTweet
                    {
                        Id = tweetId
                    }
                },
                User = new SyncTwitterUser
                {
                    Acct = userAcct
                },
                Followers = new[]
                {
                    new Follower
                    {
                        Id = userId1,
                        Host = host1,
                        InboxRoute = inbox,
                        PostingErrorCount = 50
                    },
                    new Follower
                    {
                        Id = userId2,
                        Host = host2,
                        InboxRoute = inbox,
                        PostingErrorCount = 50
                    },
                }
            };
            #endregion

            #region Mocks
            var sendTweetsToInboxTaskMock = new Mock<ISendTweetsToInboxTask>(MockBehavior.Strict);
            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId1),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Returns(Task.CompletedTask);

            sendTweetsToInboxTaskMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<ExtractedTweet[]>(y => y.Length == 1),
                    It.Is<Follower>(y => y.Id == userId2),
                    It.Is<SyncTwitterUser>(y => y.Acct == userAcct)))
                .Throws(new Exception());

            var sendTweetsToSharedInboxTaskMock = new Mock<ISendTweetsToSharedInboxTask>(MockBehavior.Strict);

            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId1 && y.PostingErrorCount == 0)))
                .Returns(Task.CompletedTask);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(It.Is<Follower>(y => y.Id == userId2 && y.PostingErrorCount == 51)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<SendTweetsToFollowersProcessor>>();
            #endregion

            var processor = new SendTweetsToFollowersProcessor(sendTweetsToInboxTaskMock.Object, sendTweetsToSharedInboxTaskMock.Object, followersDalMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(userWithTweets, CancellationToken.None);

            #region Validations
            sendTweetsToInboxTaskMock.VerifyAll();
            sendTweetsToSharedInboxTaskMock.VerifyAll();
            followersDalMock.VerifyAll();
            #endregion
        }
    }
}