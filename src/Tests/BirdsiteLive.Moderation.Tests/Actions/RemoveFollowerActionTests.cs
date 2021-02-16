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
    public class RemoveFollowerActionTests
    {
        [TestMethod]
        public async Task ProcessAsync_NoMoreFollowings()
        {
            #region Stubs
            var follower = new Follower
            {
                Id = 12,
                Followings = new List<int> { 1 }
            };
            #endregion

            #region Mocks
            var rejectAllFollowingsActionMock = new Mock<IRejectAllFollowingsAction>();
            rejectAllFollowingsActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);

            var followersDalMock = new Mock<IFollowersDal>();
            followersDalMock
                .Setup(x => x.GetFollowersAsync(
                    It.Is<int>(y => y == 1)))
                .ReturnsAsync(new[] {follower});

            followersDalMock
                .Setup(x => x.DeleteFollowerAsync(
                    It.Is<int>(y => y == 12)))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>();
            twitterUserDalMock
                .Setup(x => x.DeleteTwitterUserAsync(
                    It.Is<int>(y => y == 1)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new RemoveFollowerAction(followersDalMock.Object, twitterUserDalMock.Object, rejectAllFollowingsActionMock.Object);
            await action.ProcessAsync(follower);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            rejectAllFollowingsActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_HaveFollowings()
        {
            #region Stubs
            var follower = new Follower
            {
                Id = 12,
                Followings = new List<int> { 1 }
            };

            var followers = new List<Follower>
            {
                follower,
                new Follower
                {
                    Id = 11
                }
            };
            #endregion

            #region Mocks
            var rejectAllFollowingsActionMock = new Mock<IRejectAllFollowingsAction>();
            rejectAllFollowingsActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);

            var followersDalMock = new Mock<IFollowersDal>();
            followersDalMock
                .Setup(x => x.GetFollowersAsync(
                    It.Is<int>(y => y == 1)))
                .ReturnsAsync(followers.ToArray());

            followersDalMock
                .Setup(x => x.DeleteFollowerAsync(
                    It.Is<int>(y => y == 12)))
                .Returns(Task.CompletedTask);

            var twitterUserDalMock = new Mock<ITwitterUserDal>();
            #endregion

            var action = new RemoveFollowerAction(followersDalMock.Object, twitterUserDalMock.Object, rejectAllFollowingsActionMock.Object);
            await action.ProcessAsync(follower);

            #region Validations
            followersDalMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            rejectAllFollowingsActionMock.VerifyAll();
            #endregion
        }
    }
}