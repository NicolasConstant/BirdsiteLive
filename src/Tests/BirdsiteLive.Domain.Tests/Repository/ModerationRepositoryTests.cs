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
        public void GetModerationType_Follower_WhiteListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.WhiteListing ,repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_WhiteListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_WhiteListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "@me@domain.ext",
                TwitterAccountsWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_Follower_BlackListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlackListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.BlackListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_BlackListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlackListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.BlackListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_BlackListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlackListing = "@me@domain.ext",
                TwitterAccountsBlackListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.BlackListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.BlackListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_Follower_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlackListing = "@me@domain.ext",
                FollowersWhiteListing = "@me@domain.ext",
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_TwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlackListing = "@me@domain.ext",
                TwitterAccountsWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.None, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }

        [TestMethod]
        public void GetModerationType_FollowerTwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlackListing = "@me@domain.ext",
                FollowersWhiteListing = "@me@domain.ext",
                TwitterAccountsBlackListing = "@me@domain.ext",
                TwitterAccountsWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.Follower));
            Assert.AreEqual(ModerationTypeEnum.WhiteListing, repo.GetModerationType(ModerationEntityTypeEnum.TwitterAccount));
            #endregion
        }
        #endregion

        #region CheckStatus
        [TestMethod]
        public void CheckStatus_Follower_WhiteListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_WhiteListing_Instance_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain2.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_WhiteListing_SubDomain_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "*.domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@s.domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@s2.domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain2.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_WhiteListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsWhiteListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle2"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_BlackListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersBlackListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.BlackListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_BlackListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsBlackListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.BlackListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle2"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_Follower_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                FollowersWhiteListing = "@me@domain.ext",
                FollowersBlackListing = "@me@domain.ext"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me@domain.ext"));
            Assert.AreEqual(ModeratedTypeEnum.None, repo.CheckStatus(ModerationEntityTypeEnum.Follower, "@me2@domain.ext"));
            #endregion
        }

        [TestMethod]
        public void CheckStatus_TwitterAccount_BothListing_Test()
        {
            #region Stubs
            var settings = new ModerationSettings
            {
                TwitterAccountsWhiteListing = "handle",
                TwitterAccountsBlackListing = "handle"
            };
            #endregion

            var repo = new ModerationRepository(settings);

            #region Validations
            Assert.AreEqual(ModeratedTypeEnum.WhiteListed, repo.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, "handle"));
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