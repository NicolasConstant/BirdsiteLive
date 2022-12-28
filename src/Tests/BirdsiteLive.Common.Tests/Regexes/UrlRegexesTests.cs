using BirdsiteLive.Common.Regexes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Common.Tests
{
    [TestClass]
    public class UrlRegexesTests
    {
        [TestMethod]
        public void Url_Test()
        {
            var input = "https://misskey.tdl/users/8hwf6zy2k1#main-key";
            Assert.IsTrue(UrlRegexes.Url.IsMatch(input));
        }

        [TestMethod]
        public void Url_Not_Test()
        {
            var input = "misskey.tdl/users/8hwf6zy2k1#main-key";
            Assert.IsFalse(UrlRegexes.Url.IsMatch(input));
        }

        [TestMethod]
        public void Domain_Test()
        {
            var input = "misskey-data_sq.tdl";
            Assert.IsTrue(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_Numbers_Test()
        {
            var input = "miss45654QAzedqskey-data_sq.tdl";
            Assert.IsTrue(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_Subdomain_Test()
        {
            var input = "s.sub.dqdq-_Dz9sd.tdl";
            Assert.IsTrue(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_Not_Test()
        {
            var input = "mis$s45654QAzedqskey-data_sq.tdl";
            Assert.IsFalse(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_Slash_Test()
        {
            var input = "miss45654QAz/edqskey-data_sq.tdl";
            Assert.IsFalse(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_NotSub_Test()
        {
            var input = ".mis$s45654QAzedqskey-data_sq.tdl";
            Assert.IsFalse(UrlRegexes.Domain.IsMatch(input));
        }

        [TestMethod]
        public void Domain_NotExt_Test()
        {
            var input = ".mis$s45654QAzedqskey-data_sq.tdl";
            Assert.IsFalse(UrlRegexes.Domain.IsMatch(input));
        }
    }
}