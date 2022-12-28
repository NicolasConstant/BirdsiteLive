using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.BusinessUseCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Org.BouncyCastle.Crypto.Prng;

namespace BirdsiteLive.Domain.Tests.BusinessUseCases
{
    [TestClass]
    public class ProcessFollowUserTests
    {
        [TestMethod]
        public async Task ExecuteAsync_UserDontExists_TwitterDontExists_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";
            var followerInbox = "/user/testest";
            var inbox = "/inbox";
            var actorId = "actorUrl";

            var follower = new Follower
            {
                Id = 1,
                Acct = username,
                Host = domain,
                SharedInboxRoute = followerInbox,
                InboxRoute = inbox,
                Followings = new List<int>(),
                FollowingsSyncStatus = new Dictionary<int, long>()
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 2,
                Acct = twitterName,
                LastTweetPostedId = -1,
                LastTweetSynchronizedForAllFollowersId = -1
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .SetupSequence(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync((Follower)null)
                .ReturnsAsync(follower);

            followersDalMock
                .Setup(x => x.CreateFollowerAsync(
                    It.Is<string>(y => y == username),
                    It.Is<string>(y => y == domain),
                    It.Is<string>(y => y == followerInbox),
                    It.Is<string>(y => y == inbox),
                    It.Is<string>(y => y == actorId),
                    null,
                    null))
                .Returns(Task.CompletedTask);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(
                    It.Is<Follower>(y => y.Followings.Contains(twitterUser.Id)
                                         && y.FollowingsSyncStatus[twitterUser.Id] == -1)
                ))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .SetupSequence(x => x.GetTwitterUserAsync(twitterName))
                .ReturnsAsync((SyncTwitterUser)null)
                .ReturnsAsync(twitterUser);

            twitterUserDalMock
                .Setup(x => x.CreateTwitterUserAsync(
                    It.Is<string>(y => y == twitterName),
                    It.Is<long>(y => y == -1),
                    It.Is<string>(y => y == null),
                    It.Is<string>(y => y == null)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new ProcessFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName, followerInbox, inbox, actorId);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_UserExists_TwitterExists_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";
            var followerInbox = "/user/testest";
            var inbox = "/inbox";
            var actorId = "actorUrl";
            
            var follower = new Follower
            {
                Id = 1,
                Acct = username,
                Host = domain,
                SharedInboxRoute = followerInbox,
                InboxRoute = inbox,
                Followings = new List<int>(),
                FollowingsSyncStatus = new Dictionary<int, long>()
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 2,
                Acct = twitterName,
                LastTweetPostedId = -1,
                LastTweetSynchronizedForAllFollowersId = -1
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync(follower);
            
            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(
                    It.Is<Follower>(y => y.Followings.Contains(twitterUser.Id)
                                         && y.FollowingsSyncStatus[twitterUser.Id] == -1)
                ))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(twitterName))
                .ReturnsAsync(twitterUser);
            #endregion

            var action = new ProcessFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName, followerInbox, inbox, actorId);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }
    }
}