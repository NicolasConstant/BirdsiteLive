using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Processors;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors
{
    [TestClass]
    public class RetrieveTwitterUsersProcessorTests
    {
        [TestMethod]
        public async Task GetTwitterUsersAsync_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();
            var users = new[]
            {
                new SyncTwitterUser(),
                new SyncTwitterUser(),
                new SyncTwitterUser(),
            };
            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 10
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == settings.MaxUsersCapacity)))
                .ReturnsAsync(users);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 10;
            processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.Delay(50);

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(3, buffer.Count);
            buffer.TryReceive(out var result);
            Assert.AreEqual(1, result.Length);
            #endregion
        }

        [TestMethod]
        public async Task GetTwitterUsersAsync_Multi_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();
            var users = new List<SyncTwitterUser>();

            for (var i = 0; i < 30; i++)
                users.Add(new SyncTwitterUser());

            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 100
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .SetupSequence(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == settings.MaxUsersCapacity)))
                .ReturnsAsync(users.ToArray())
                .ReturnsAsync(new SyncTwitterUser[0]);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 2;
            processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.Delay(300);

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(15, buffer.Count);
            buffer.TryReceive(out var result);
            Assert.AreEqual(2, result.Length);
            #endregion
        }

        [TestMethod]
        public async Task GetTwitterUsersAsync_Multi2_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();
            var users = new List<SyncTwitterUser>();

            for (var i = 0; i < 31; i++)
                users.Add(new SyncTwitterUser());

            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 10
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .SetupSequence(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == settings.MaxUsersCapacity)))
                .ReturnsAsync(users.ToArray())
                .ReturnsAsync(new SyncTwitterUser[0]);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 2;
            processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.Delay(200);

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(11, buffer.Count);
            buffer.TryReceive(out var result);
            Assert.AreEqual(3, result.Length);
            #endregion
        }

        [TestMethod]
        public async Task GetTwitterUsersAsync_NoUsers_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();

            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 10
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == settings.MaxUsersCapacity)))
                .ReturnsAsync(new SyncTwitterUser[0]);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 1;
            processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.Delay(50);

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(0, buffer.Count);
            #endregion
        }
        
        [TestMethod]
        public async Task GetTwitterUsersAsync_Exception_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();

            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 10
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == settings.MaxUsersCapacity)))
                .Returns(async () => await DelayFaultedTask<SyncTwitterUser[]>(new Exception()));

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 10;
            var t = processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.WhenAny(t, Task.Delay(50));

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(0, buffer.Count);
            #endregion
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task GetTwitterUsersAsync_Cancellation_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();
            var canTokenS = new CancellationTokenSource();
            canTokenS.Cancel();

            var settings = new InstanceSettings
            {
                MaxUsersCapacity = 10
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, settings, loggerMock.Object);
            processor.WaitFactor = 1;
            await processor.GetTwitterUsersAsync(buffer, canTokenS.Token);
        }

        private static async Task<T> DelayFaultedTask<T>(Exception e)
        {
            await Task.Delay(30);
            throw e;
        }
    }
}