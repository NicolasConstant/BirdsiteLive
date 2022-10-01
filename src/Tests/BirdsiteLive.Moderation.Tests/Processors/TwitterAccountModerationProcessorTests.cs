using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Moderation.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Moderation.Tests.Processors
{
    [TestClass]
    public class TwitterAccountModerationProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_None()
        {
            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new TwitterAccountModerationProcessor(twitterUserDalMock.Object, moderationRepositoryMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.None);

            #region Validations
            twitterUserDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_AllowListing_AllowListed()
        {
            #region Stubs
            var allUsers = new List<SyncTwitterUser>
            {
                new SyncTwitterUser
                {
                    Acct = "acct"
                }
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(allUsers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.TwitterAccount),
                    It.Is<string>(y => y == "acct")))
                .Returns(ModeratedTypeEnum.AllowListed);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new TwitterAccountModerationProcessor(twitterUserDalMock.Object, moderationRepositoryMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.AllowListing);

            #region Validations
            twitterUserDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_AllowListing_NotAllowListed()
        {
            #region Stubs
            var allUsers = new List<SyncTwitterUser>
            {
                new SyncTwitterUser
                {
                    Acct = "acct"
                }
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(allUsers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.TwitterAccount),
                    It.Is<string>(y => y == "acct")))
                .Returns(ModeratedTypeEnum.None);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            removeTwitterAccountActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<SyncTwitterUser>(y => y.Acct == "acct")))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new TwitterAccountModerationProcessor(twitterUserDalMock.Object, moderationRepositoryMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.AllowListing);

            #region Validations
            twitterUserDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_BlockListing_BlockListed()
        {
            #region Stubs
            var allUsers = new List<SyncTwitterUser>
            {
                new SyncTwitterUser
                {
                    Acct = "acct"
                }
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(allUsers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.TwitterAccount),
                    It.Is<string>(y => y == "acct")))
                .Returns(ModeratedTypeEnum.BlockListed);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            removeTwitterAccountActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<SyncTwitterUser>(y => y.Acct == "acct")))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new TwitterAccountModerationProcessor(twitterUserDalMock.Object, moderationRepositoryMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.BlockListing);

            #region Validations
            twitterUserDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_BlockListing_NotBlockListed()
        {
            #region Stubs
            var allUsers = new List<SyncTwitterUser>
            {
                new SyncTwitterUser
                {
                    Acct = "acct"
                }
            };
            #endregion

            #region Mocks
            var twitterUserDalMock = new Mock<ITwitterUserDal>(MockBehavior.Strict);
            twitterUserDalMock
                .Setup(x => x.GetAllTwitterUsersAsync())
                .ReturnsAsync(allUsers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.TwitterAccount),
                    It.Is<string>(y => y == "acct")))
                .Returns(ModeratedTypeEnum.None);

            var removeTwitterAccountActionMock = new Mock<IRemoveTwitterAccountAction>(MockBehavior.Strict);
            #endregion

            var processor = new TwitterAccountModerationProcessor(twitterUserDalMock.Object, moderationRepositoryMock.Object, removeTwitterAccountActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.BlockListing);

            #region Validations
            twitterUserDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeTwitterAccountActionMock.VerifyAll();
            #endregion
        }
    }
}