using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Twitter;
using Castle.DynamicProxy.Generators.Emitters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BirdsiteLive.Pipeline.Tests.Processors
{
    [TestClass]
    class RetrieveTweetsProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_Test() 
        {
            var users = new List<SyncTwitterUser>
            {
                new SyncTwitterUser { Id = 1 },
                new SyncTwitterUser { Id = 2 },
                new SyncTwitterUser { Id = 3 },
            };

            var twitterServiceMock = new Mock<ITwitterService>(MockBehavior.Strict);
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);

            var procesor = new RetrieveTweetsProcessor(twitterServiceMock.Object, twitterUserDalMock.Object);

            var result = await procesor.ProcessAsync(users.ToArray(), CancellationToken.None);



        }

    }
}
