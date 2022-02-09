using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.BusinessUseCases;
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
            var rejectAllFollowingsActionMock = new Mock<IRejectAllFollowingsAction>(MockBehavior.Strict);
            rejectAllFollowingsActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);

            var processDeleteUserMock = new Mock<IProcessDeleteUser>(MockBehavior.Strict);
            processDeleteUserMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new RemoveFollowerAction(rejectAllFollowingsActionMock.Object, processDeleteUserMock.Object);
            await action.ProcessAsync(follower);

            #region Validations
            rejectAllFollowingsActionMock.VerifyAll();
            processDeleteUserMock.VerifyAll();
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
            #endregion

            #region Mocks
            var rejectAllFollowingsActionMock = new Mock<IRejectAllFollowingsAction>(MockBehavior.Strict);
            rejectAllFollowingsActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);

            var processDeleteUserMock = new Mock<IProcessDeleteUser>(MockBehavior.Strict);
            processDeleteUserMock
                .Setup(x => x.ExecuteAsync(
                    It.Is<Follower>(y => y.Id == follower.Id)))
                .Returns(Task.CompletedTask);
            #endregion

            var action = new RemoveFollowerAction(rejectAllFollowingsActionMock.Object, processDeleteUserMock.Object);
            await action.ProcessAsync(follower);

            #region Validations
            rejectAllFollowingsActionMock.VerifyAll();
            processDeleteUserMock.VerifyAll();
            #endregion
        }
    }
}