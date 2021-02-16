using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Moderation.Tests.Actions
{
    [TestClass]
    public class RemoveTwitterAccountActionTests
    {
        [TestMethod]
        public async Task ProcessAsync_RemoveFollower()
        {
            #region Stubs
            var twitter = new SyncTwitterUser
            {
                Id = 24,
                Acct = "my-acct"
            };

            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 48,
                    Followings = new List<int>{ 24 },
                    FollowingsSyncStatus = new Dictionary<int, long> { { 24, 1024 } }
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowersAsync(
                    It.Is<int>(y => y == 24)))
                .ReturnsAsync(followers.ToArray());

            followersDalMock
                .Setup(x => x.DeleteFollowerAsync(
                    It.Is<int>(y => y == 48)))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.DeleteTwitterUserAsync(
                    It.Is<int>(y => y == 24)))
                .Returns(Task.CompletedTask);

            var rejectFollowingActionMock = new Mock<IRejectFollowingAction>(MockBehavior.Strict);
            rejectFollowingActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == 48),
                    It.Is<SyncTwitterUser>(y => y.Acct == twitter.Acct)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new RemoveTwitterAccountAction(followersDalMock.Object, twitterUserDalMock.Object, rejectFollowingActionMock.Object);
            await action.ProcessAsync(twitter);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            rejectFollowingActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_KeepFollower()
        {
            #region Stubs
            var twitter = new SyncTwitterUser
            {
                Id = 24,
                Acct = "my-acct"
            };

            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 48,
                    Followings = new List<int>{ 24, 36 },
                    FollowingsSyncStatus = new Dictionary<int, long> { { 24, 1024 }, { 36, 24 } }
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetFollowersAsync(
                    It.Is<int>(y => y == 24)))
                .ReturnsAsync(followers.ToArray());

            followersDalMock
                .Setup(x => x.UpdateFollowerAsync(
                    It.Is<Follower>(y => y.Id == 48
                        && y.Followings.Count == 1
                        && y.FollowingsSyncStatus.Count == 1
                    )))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.DeleteTwitterUserAsync(
                    It.Is<int>(y => y == 24)))
                .Returns(Task.CompletedTask);

            var rejectFollowingActionMock = new Mock<IRejectFollowingAction>(MockBehavior.Strict);
            rejectFollowingActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == 48),
                    It.Is<SyncTwitterUser>(y => y.Acct == twitter.Acct)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new RemoveTwitterAccountAction(followersDalMock.Object, twitterUserDalMock.Object, rejectFollowingActionMock.Object);
            await action.ProcessAsync(twitter);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            rejectFollowingActionMock.VerifyAll();
            #endregion
        }
    }
}