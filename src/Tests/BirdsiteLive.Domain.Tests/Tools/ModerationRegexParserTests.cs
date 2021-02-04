using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Domain.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Domain.Tests.Tools
{
    [TestClass]
    public class ModerationRegexParserTests
    {
        [TestMethod]
        public void Parse_TwitterAccount_Simple_Test()
        {
            #region Stubs
            var pattern = "handle";
            #endregion

            var regex = ModerationRegexParser.Parse(ModerationEntityTypeEnum.TwitterAccount, pattern);

            #region Validations
            Assert.IsTrue(regex.IsMatch(pattern));
            Assert.IsFalse(regex.IsMatch("handles"));
            Assert.IsFalse(regex.IsMatch("andle"));
            #endregion
        }

        [TestMethod]
        public void Parse_Follower_Handle_Test()
        {
            #region Stubs
            var pattern = "@handle@domain.ext";
            #endregion

            var regex = ModerationRegexParser.Parse(ModerationEntityTypeEnum.Follower, pattern);

            #region Validations
            Assert.IsTrue(regex.IsMatch(pattern));
            Assert.IsFalse(regex.IsMatch("@handle2@domain.ext"));
            Assert.IsFalse(regex.IsMatch("@handle@seb.domain.ext"));
            #endregion
        }

        [TestMethod]
        public void Parse_Follower_Domain_Test()
        {
            #region Stubs
            var pattern = "domain.ext";
            #endregion

            var regex = ModerationRegexParser.Parse(ModerationEntityTypeEnum.Follower, pattern);

            #region Validations
            Assert.IsTrue(regex.IsMatch("@handle@domain.ext"));
            Assert.IsTrue(regex.IsMatch("@handle2@domain.ext"));
            Assert.IsFalse(regex.IsMatch("@handle2@domain2.ext"));
            Assert.IsFalse(regex.IsMatch("@handle@seb.domain.ext"));
            #endregion
        }

        [TestMethod]
        public void Parse_Follower_SubDomains_Test()
        {
            #region Stubs
            var pattern = "*.domain.ext";
            #endregion

            var regex = ModerationRegexParser.Parse(ModerationEntityTypeEnum.Follower, pattern);

            #region Validations
            Assert.IsTrue(regex.IsMatch("@handle2@sub.domain.ext"));
            Assert.IsTrue(regex.IsMatch("@han@sub3.domain.ext"));
            Assert.IsFalse(regex.IsMatch("@handle@domain.ext"));
            Assert.IsFalse(regex.IsMatch("@handle2@.domain.ext"));
            Assert.IsFalse(regex.IsMatch("@handle2@domain2.ext"));
            Assert.IsFalse(regex.IsMatch("@handle@seb.domain2.ext"));
            #endregion
        }
    }
}