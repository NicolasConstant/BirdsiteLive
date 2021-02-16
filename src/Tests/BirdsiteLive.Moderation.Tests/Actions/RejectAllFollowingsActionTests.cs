using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.ActivityPub;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Moderation.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Moderation.Tests.Actions
{
    [TestClass]
    public class RejectAllFollowingsActionTests
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
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(
                    It.Is<int>(y => y == 24)))
                .ReturnsAsync(new SyncTwitterUser
                {
                    Id = 24,
                    Acct = "acct"
                });

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(x => x.SendRejectFollowAsync(
                    It.Is<ActivityFollow>(y => y.type == "Follow"),
                    It.IsNotNull<string>()
                ))
                .ReturnsAsync(true);
            #endregion

            var action = new RejectAllFollowingsAction(twitterUserDalMock.Object, userServiceMock.Object, settings);
            await action.ProcessAsync(follower);

            #region Validations
            twitterUserDalMock.VerifyAll();
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
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUserAsync(
                    It.Is<int>(y => y == 24)))
                .ReturnsAsync(new SyncTwitterUser
                {
                    Id = 24,
                    Acct = "acct"
                });

            var userServiceMock = new Mock<IUserService>(MockBehavior.Strict);
            userServiceMock
                .Setup(x => x.SendRejectFollowAsync(
                    It.Is<ActivityFollow>(y => y.type == "Follow"),
                    It.IsNotNull<string>()
                ))
                .Throws(new Exception());
            #endregion

            var action = new RejectAllFollowingsAction(twitterUserDalMock.Object, userServiceMock.Object, settings);
            await action.ProcessAsync(follower);

            #region Validations
            twitterUserDalMock.VerifyAll();
            userServiceMock.VerifyAll();
            #endregion
        }
    }
}