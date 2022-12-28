using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BirdsiteLive.Common.Regexes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Domain.Tests
{
    [TestClass]
    public class TheFedInfoServiceTests
    {
        [TestMethod]
        public async Task GetBslInstanceListAsyncTest()
        {
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient());

            var service = new TheFedInfoService(httpClientFactoryMock.Object);

            var bslInstanceList = await service.GetBslInstanceListAsync();

            Assert.IsTrue(bslInstanceList.Count > 0);

            foreach (var instanceInfo in bslInstanceList)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(instanceInfo.Host));
                Assert.IsTrue(UrlRegexes.Domain.IsMatch(instanceInfo.Host));
                Assert.IsTrue(instanceInfo.Version > new Version(0, 1, 0));
            }
        }
    }
}