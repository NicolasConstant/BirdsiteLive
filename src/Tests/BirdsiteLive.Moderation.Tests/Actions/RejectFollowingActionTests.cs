using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Moderation.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Moderation.Tests.Actions
{
    [TestClass]
    public class RejectFollowingActionTests
    {
        [TestMethod]
        public async Task ProcessAsync()
        {
            #region Stubs
            var follower = new Follower
            {
                Followings = new List<int>
                {
                    24
                },
                Host = "host"
            };

            var settings = new InstanceSettings
            {
                Domain = "domain"
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 24,
                Acct = "acct"
            };
            #endregion

            #region Mocks
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(x => x.SendRejectFollowAsync(
                    It.Is<ActivityFollow>(y => y.type == "Follow"),
                    It.IsNotNull<string>()
                ))
                .ReturnsAsync(true);
            #endregion

            var action = new RejectFollowingAction(userServiceMock.Object, settings);
            await action.ProcessAsync(follower, twitterUser);

            #region Validations
            userServiceMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_Exception()
        {
            #region Stubs
            var follower = new Follower
            {
                Followings = new List<int>
                {
                    24
                },
                Host = "host"
            };

            var settings = new InstanceSettings
            {
                Domain = "domain"
            };

            var twitterUser = new SyncTwitterUser
            {
                Id = 24,
                Acct = "acct"
            };
            #endregion

            #region Mocks
            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(x => x.SendRejectFollowAsync(
                    It.Is<ActivityFollow>(y => y.type == "Follow"),
                    It.IsNotNull<string>()
                ))
                .Throws(new Exception());
            #endregion

            var action = new RejectFollowingAction(userServiceMock.Object, settings);
            await action.ProcessAsync(follower, twitterUser);

            #region Validations
            userServiceMock.VerifyAll();
            #endregion
        }
    }
}