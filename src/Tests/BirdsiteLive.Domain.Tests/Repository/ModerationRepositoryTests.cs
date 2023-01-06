using BirdsiteLive.Common.Settings;
using BirdsiteLive.Domain.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Domain.Tests.Repository
{
    [TestClass]
    public class ModerationRepositoryTests
    {
        #region GetModerationType
        [TestMethod]
        public void GetModerationType_Follower_AllowListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.AllowListing ,repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_AllowListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_AllowListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "@me@domain.ext",
                TwitterAccountsAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_Follower_BlockListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlockListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.BlockListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_BlockListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlockListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.BlockListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_BlockListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlockListing = "@me@domain.ext",
                TwitterAccountsBlockListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.BlockListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.BlockListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_Follower_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlockListing = "@me@domain.ext",
                FollowersAllowListing = "@me@domain.ext",
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlockListing = "@me@domain.ext",
                TwitterAccountsAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlockListing = "@me@domain.ext",
                FollowersAllowListing = "@me@domain.ext",
                TwitterAccountsBlockListing = "@me@domain.ext",
                TwitterAccountsAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.AllowListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }
        #endregion

        #region CheckStatus
        [TestMethod]
        public void CheckStatus_Follower_AllowListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_AllowListing_Instance_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain2.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_AllowListing_SubDomain_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "*.domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@s.domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@s2.domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain2.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_AllowListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsAllowListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle2"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_BlockListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlockListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.BlockListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_BlockListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlockListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.BlockListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle2"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersAllowListing = "@me@domain.ext",
                FollowersBlockListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsAllowListing = "handle",
                TwitterAccountsBlockListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.AllowListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle2"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_None_Test()
        {
            #region Stubs
            var settings = new ModerationSettings();
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@handle@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            #endregion
        }
        #endregion
    }
}