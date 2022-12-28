using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Processors;
using BirdsiteLive.Pipeline.Tools;
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
            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == maxUsers),
                    It.Is<bool>(y => y == false)))
                .ReturnsAsync(users);
            
            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
            processor.WaitFactor = 10;
            var t = processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.WhenAny(t, Task.Delay(50));

            #region Validations
            maxUsersNumberProviderMock.VerifyAll();
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

            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .SetupSequence(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == maxUsers),
                    It.Is<bool>(y => y == false)))
                .ReturnsAsync(users.ToArray())
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0]);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
            processor.WaitFactor = 2;
            var t = processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.WhenAny(t, Task.Delay(300));
            
            #region Validations
            maxUsersNumberProviderMock.VerifyAll();
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

            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .SetupSequence(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == maxUsers),
                    It.Is<bool>(y => y == false)))
                .ReturnsAsync(users.ToArray())
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0])
                .ReturnsAsync(new SyncTwitterUser[0]);
            
            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
            processor.WaitFactor = 2;
            var t = processor.GetTwitterUsersAsync(buffer, CancellationToken.None);
            var t2 = Task.Run(async () =>
            {
                while (buffer.Count < 11)
                    await Task.Delay(50);
            });

            await Task.WhenAny(t, t2, Task.Delay(5000));

            #region Validations
            maxUsersNumberProviderMock.VerifyAll();
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

            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == maxUsers),
                    It.Is<bool>(y => y == false)))
                .ReturnsAsync(new SyncTwitterUser[0]);

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
            processor.WaitFactor = 1;
            var t =processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.WhenAny(t, Task.Delay(50));

            #region Validations
            maxUsersNumberProviderMock.VerifyAll();
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(0, buffer.Count);
            #endregion
        }
        
        [TestMethod]
        public async Task GetTwitterUsersAsync_Exception_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();

            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync(
                    It.Is<int>(y => y == maxUsers),
                    It.Is<bool>(y => y == false)))
                .Returns(async () => await DelayFaultedTask<SyncTwitterUser[]>(new Exception()));

            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
            processor.WaitFactor = 10;
            var t = processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.WhenAny(t, Task.Delay(50));

            #region Validations
            maxUsersNumberProviderMock.VerifyAll();
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

            var maxUsers = 1000;
            #endregion

            #region Mocks
            var maxUsersNumberProviderMock = new Mock<IMaxUsersNumberProvider>(MockBehavior.Strict);
            maxUsersNumberProviderMock
                .Setup(x => x.GetMaxUsersNumberAsync())
                .ReturnsAsync(maxUsers);

            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            
            var loggerMock = new Mock<ILogger<RetrieveTwitterUsersProcessor>>();
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object, maxUsersNumberProviderMock.Object, loggerMock.Object);
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