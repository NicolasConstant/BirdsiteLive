using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Processors;
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
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(users);
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object);
            processor.GetTwitterUsersAsync(buffer, CancellationToken.None);

            await Task.Delay(50);

            #region Validations
            twitterUserDalMock.VerifyAll();
            Assert.AreEqual(1, buffer.Count);
            buffer.TryReceive(out var result);
            Assert.AreEqual(3, result.Length);
            #endregion
        }

        [TestMethod]
        public async Task GetTwitterUsersAsync_NoUsers_Test()
        {
            #region Stubs
            var buffer = new BufferBlock<SyncTwitterUser[]>();
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(new SyncTwitterUser[0]);
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object);
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
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .Throws(new Exception());
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object);
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
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            #endregion

            var processor = new RetrieveTwitterUsersProcessor(twitterUserDalMock.Object);
            await processor.GetTwitterUsersAsync(buffer, canTokenS.Token);
        }
    }
}