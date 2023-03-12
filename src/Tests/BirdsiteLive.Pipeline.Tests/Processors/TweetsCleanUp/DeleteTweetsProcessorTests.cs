using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.Federation;
using BirdsiteLive.Pipeline.Processors.TweetsCleanUp;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Pipeline.Tests.Processors.TweetsCleanUp
{
    [TestClass]
    public class DeleteTweetsProcessorTests
    {
        [TestMethod]
        public async Task Process_DeleteTweet_Test()
        {
            #region Stubs
            var tweetId = 42;
            var tweet = new TweetToDelete
            {
                Tweet = new SyncTweet
                {
                    Id = tweetId
                }
            };
            #endregion

            #region Mocks
            var serviceMock = new Mock<IActivityPubService>(MockBehavior.Strict);
            serviceMock
                .Setup(x => x.DeleteNoteAsync(It.Is<SyncTweet>(y => y.Id == tweetId)))
                .Returns(Task.CompletedTask);

            var loggerMock = new Mock<ILogger<DeleteTweetsProcessor>>();
            #endregion

            var processor = new DeleteTweetsProcessor(serviceMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(tweet, CancellationToken.None);

            #region Validations
            serviceMock.VerifyAll();
            loggerMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.DeleteSuccessful);
            #endregion
        }

        [TestMethod]
        public async Task Process_DeleteTweet_NotFound_Test()
        {
            #region Stubs
            var tweetId = 42;
            var tweet = new TweetToDelete
            {
                Tweet = new SyncTweet
                {
                    Id = tweetId
                }
            };
            #endregion

            #region Mocks
            var serviceMock = new Mock<IActivityPubService>(MockBehavior.Strict);
            serviceMock
                .Setup(x => x.DeleteNoteAsync(It.Is<SyncTweet>(y => y.Id == tweetId)))
                .Throws(new HttpRequestException("not found", null, HttpStatusCode.NotFound));

            var loggerMock = new Mock<ILogger<DeleteTweetsProcessor>>();
            #endregion

            var processor = new DeleteTweetsProcessor(serviceMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(tweet, CancellationToken.None);

            #region Validations
            serviceMock.VerifyAll();
            loggerMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.DeleteSuccessful);
            #endregion
        }

        [TestMethod]
        public async Task Process_DeleteTweet_HttpError_Test()
        {
            #region Stubs
            var tweetId = 42;
            var tweet = new TweetToDelete
            {
                Tweet = new SyncTweet
                {
                    Id = tweetId
                }
            };
            #endregion

            #region Mocks
            var serviceMock = new Mock<IActivityPubService>(MockBehavior.Strict);
            serviceMock
                .Setup(x => x.DeleteNoteAsync(It.Is<SyncTweet>(y => y.Id == tweetId)))
                .Throws(new HttpRequestException("bad gateway", null, HttpStatusCode.BadGateway));

            var loggerMock = new Mock<ILogger<DeleteTweetsProcessor>>();
            #endregion

            var processor = new DeleteTweetsProcessor(serviceMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(tweet, CancellationToken.None);

            #region Validations
            serviceMock.VerifyAll();
            loggerMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.DeleteSuccessful);
            #endregion
        }

        [TestMethod]
        public async Task Process_DeleteTweet_GenericException_Test()
        {
            #region Stubs
            var tweetId = 42;
            var tweet = new TweetToDelete
            {
                Tweet = new SyncTweet
                {
                    Id = tweetId
                }
            };
            #endregion

            #region Mocks
            var serviceMock = new Mock<IActivityPubService>(MockBehavior.Strict);
            serviceMock
                .Setup(x => x.DeleteNoteAsync(It.Is<SyncTweet>(y => y.Id == tweetId)))
                .Throws(new Exception());

            var loggerMock = new Mock<ILogger<DeleteTweetsProcessor>>();
            #endregion

            var processor = new DeleteTweetsProcessor(serviceMock.Object, loggerMock.Object);
            var result = await processor.ProcessAsync(tweet, CancellationToken.None);

            #region Validations
            serviceMock.VerifyAll();
            loggerMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsFalse(result.DeleteSuccessful);
            #endregion
        }
    }
}