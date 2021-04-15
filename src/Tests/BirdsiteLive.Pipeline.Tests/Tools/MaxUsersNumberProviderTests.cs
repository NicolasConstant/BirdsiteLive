using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Tools
{
    [TestClass]
    public class MaxUsersNumberProviderTests
    {
        [TestMethod]
        public async Task GetMaxUsersNumberAsync_WarmUp_Test()
        {
            #region Stubs
            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 1000
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUsersCountAsync())
                .ReturnsAsync(1000);
            #endregion

            var provider = new MaxUsersNumberProvider(settings, twitterUserDalMock.Object);
            
            var result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(200, result);

            result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(200, result);

            result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(200, result);

            result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(200, result);
            
            result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(200, result);
            
            result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(1000, result);
            
            #region Validations
            twitterUserDalMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task GetMaxUsersNumberAsync_NoWarmUp_Test()
        {
            #region Stubs
            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 1000
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetTwitterUsersCountAsync())
                .ReturnsAsync(199);
            #endregion

            var provider = new MaxUsersNumberProvider(settings, twitterUserDalMock.Object);

            var result = await provider.GetMaxUsersNumberAsync();
            Assert.AreEqual(1000, result);

            #region Validations
            twitterUserDalMock.VerifyAll();
            #endregion
        }
    }
}