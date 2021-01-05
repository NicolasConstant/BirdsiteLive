using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.BusinessUseCases;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Domain.Tests.BusinessUseCases
{
    [TestClass]
    public class ProcessUnfollowUserTests
    {
        [TestMethod]
        public async Task ExecuteAsync_NoFollowerFound_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync((Follower) null);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            #endregion

            var action = new ProcessUndoFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName );

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_NoTwitterUserFound_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";

            var follower = new Follower
            {
                Id = 1,
                Acct = username,
                Host = domain,
                Followings = new List<int>(),
                FollowingsSyncStatus = new Dictionary<int, long>()
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync(follower);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(twitterName))
                .ReturnsAsync((SyncTwitterUser)null);
            #endregion

            var action = new ProcessUndoFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_MultiFollows_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";

            var follower = new Follower
            {
                Id = 1,
                Acct = username,
                Host = domain,
                Followings = new List<int> { 2, 3 },
                FollowingsSyncStatus = new Dictionary<int, long> { { 2, 460 }, { 3, 563} }
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 2,
                Acct = twitterName,
                LastTweetPostedId = 460,
                LastTweetSynchronizedForAllFollowersId = 460
            };

            var followerList = new List<Follower>
            {
                new Follower(),
                new Follower()
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync(follower);

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(
                    It.Is<Follower>(y => !y.Followings.Contains(twitterUser.Id)
                                         && !y.FollowingsSyncStatus.ContainsKey(twitterUser.Id))
                ))
                .Returns(Task.CompletedTask);

            followersDalMock
                .Setup(x => x.GetFollowersAsync(twitterUser.Id))
                .ReturnsAsync(followerList.ToArray());

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(twitterName))
                .ReturnsAsync(twitterUser);
            #endregion

            var action = new ProcessUndoFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ExecuteAsync_CleanUp_Test()
        {
            #region Stubs
            var username = "testest";
            var domain = "m.s";
            var twitterName = "handle";

            var follower = new Follower
            {
                Id = 1,
                Acct = username,
                Host = domain,
                Followings = new List<int> { 2 },
                FollowingsSyncStatus = new Dictionary<int, long> { { 2, 460 } }
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 2,
                Acct = twitterName,
                LastTweetPostedId = 460,
                LastTweetSynchronizedForAllFollowersId = 460
            };

            var followerList = new List<Follower>();
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowerAsync(username, domain))
                .ReturnsAsync(follower);

            followersDalMock
                .Setup(x => x.DeleteFollowerAsync(
                    It.Is<string>(y => y == username),
                    It.Is<string>(y => y == domain)
                    ))
                .Returns(Task.CompletedTask);

            followersDalMock
                .Setup(x => x.GetFollowersAsync(twitterUser.Id))
                .ReturnsAsync(followerList.ToArray());

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(twitterName))
                .ReturnsAsync(twitterUser);

            twitterUserDalMock
                .Setup(x => x.DeleteTwitterUserAsync(
                    It.Is<string>(y => y == twitterName)
                ))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new ProcessUndoFollowUser(followersDalMock.Object, twitterUserDalMock.Object);
            await action.ExecuteAsync(username, domain, twitterName);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            #endregion
        }
    }
}