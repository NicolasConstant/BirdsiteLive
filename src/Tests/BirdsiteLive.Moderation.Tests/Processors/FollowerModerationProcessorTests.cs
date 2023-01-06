using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Moderation.Actions;
using BirdsiteLive.Moderation.Processors;
using Castle.DynamicProxy.Generators.Emitters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BirdsiteLive.Moderation.Tests.Processors
{
    [TestClass]
    public class FollowerModerationProcessorTests
    {
        [TestMethod]
        public async Task ProcessAsync_None()
        {
            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            var removeFollowerActionMock = new Mock<IRemoveFollowerAction>(MockBehavior.Strict);
            #endregion

            var processor = new FollowerModerationProcessor(followersDalMock.Object, moderationRepositoryMock.Object, removeFollowerActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.None);

            #region Validations
            followersDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeFollowerActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_AllowListing_AllowListed()
        {
            #region Stubs
            var allFollowers = new List<Follower>
            {
                new Follower
                {
                    Acct = "acct",
                    Host = "host"
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetAllFollowersAsync())
                .ReturnsAsync(allFollowers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.Follower),
                    It.Is<string>(y => y == "@acct@host")))
                .Returns(ModeratedTypeEnum.AllowListed);

            var removeFollowerActionMock = new Mock<IRemoveFollowerAction>(MockBehavior.Strict);
            #endregion

            var processor = new FollowerModerationProcessor(followersDalMock.Object, moderationRepositoryMock.Object, removeFollowerActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.AllowListing);

            #region Validations
            followersDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeFollowerActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_AllowListing_NotAllowListed()
        {
            #region Stubs
            var allFollowers = new List<Follower>
            {
                new Follower
                {
                    Acct = "acct",
                    Host = "host"
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetAllFollowersAsync())
                .ReturnsAsync(allFollowers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.Follower),
                    It.Is<string>(y => y == "@acct@host")))
                .Returns(ModeratedTypeEnum.None);

            var removeFollowerActionMock = new Mock<IRemoveFollowerAction>(MockBehavior.Strict);
            removeFollowerActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Acct == "acct")))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new FollowerModerationProcessor(followersDalMock.Object, moderationRepositoryMock.Object, removeFollowerActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.AllowListing);

            #region Validations
            followersDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeFollowerActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_BlockListing_BlockListed()
        {
            #region Stubs
            var allFollowers = new List<Follower>
            {
                new Follower
                {
                    Acct = "acct",
                    Host = "host"
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetAllFollowersAsync())
                .ReturnsAsync(allFollowers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.Follower),
                    It.Is<string>(y => y == "@acct@host")))
                .Returns(ModeratedTypeEnum.BlockListed);

            var removeFollowerActionMock = new Mock<IRemoveFollowerAction>(MockBehavior.Strict);
            removeFollowerActionMock
                .Setup(x => x.ProcessAsync(
                    It.Is<Follower>(y => y.Acct == "acct")))
                .Returns(Task.CompletedTask);
            #endregion

            var processor = new FollowerModerationProcessor(followersDalMock.Object, moderationRepositoryMock.Object, removeFollowerActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.BlockListing);

            #region Validations
            followersDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeFollowerActionMock.VerifyAll();
            #endregion
        }

        [TestMethod]
        public async Task ProcessAsync_BlockListing_NotBlockListed()
        {
            #region Stubs
            var allFollowers = new List<Follower>
            {
                new Follower
                {
                    Acct = "acct",
                    Host = "host"
                }
            };
            #endregion

            #region Mocks
            var followersDalMock = new Mock<IFollowersDal>(MockBehavior.Strict);
            followersDalMock
                .Setup(x => x.GetAllFollowersAsync())
                .ReturnsAsync(allFollowers.ToArray());

            var moderationRepositoryMock = new Mock<IModerationRepository>(MockBehavior.Strict);
            moderationRepositoryMock
                .Setup(x => x.CheckStatus(
                    It.Is<ModerationEntityTypeEnum>(y => y == ModerationEntityTypeEnum.Follower),
                    It.Is<string>(y => y == "@acct@host")))
                .Returns(ModeratedTypeEnum.None);

            var removeFollowerActionMock = new Mock<IRemoveFollowerAction>(MockBehavior.Strict);
            #endregion

            var processor = new FollowerModerationProcessor(followersDalMock.Object, moderationRepositoryMock.Object, removeFollowerActionMock.Object);
            await processor.ProcessAsync(ModerationTypeEnum.BlockListing);

            #region Validations
            followersDalMock.VerifyAll();
            moderationRepositoryMock.VerifyAll();
            removeFollowerActionMock.VerifyAll();
            #endregion
        }
    }
}