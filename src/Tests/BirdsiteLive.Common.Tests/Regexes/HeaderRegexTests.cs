using BirdsiteLive.Common.Regexes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Common.Tests
{
    [TestClass]
    public class HeaderRegexTests
    {
        [TestMethod]
        public void KeyId_Test()
        {
            var input = @"keyId=""https://misskey.tdl/users/8hwf6zy2k1#main-key""";

            Assert.IsTrue(HeaderRegexes.HeaderSignature.IsMatch(input));
            var result = HeaderRegexes.HeaderSignature.Match(input);
            Assert.AreEqual("keyId", result.Groups[1].ToString());
            Assert.AreEqual("https://misskey.tdl/users/8hwf6zy2k1#main-key", result.Groups[2].ToString());
        }

        [TestMethod]
        public void Algorithm_Test()
        {
            var input = @"algorithm=""rsa-sha256""";

            Assert.IsTrue(HeaderRegexes.HeaderSignature.IsMatch(input));
            var result = HeaderRegexes.HeaderSignature.Match(input);
            Assert.AreEqual("algorithm", result.Groups[1].ToString());
            Assert.AreEqual("rsa-sha256", result.Groups[2].ToString());
        }

        [TestMethod]
        public void Target_Test()
        {
            var input = @"headers=""(request-target) date host digest""";

            Assert.IsTrue(HeaderRegexes.HeaderSignature.IsMatch(input));
            var result = HeaderRegexes.HeaderSignature.Match(input);
            Assert.AreEqual("headers", result.Groups[1].ToString());
            Assert.AreEqual("(request-target) date host digest", result.Groups[2].ToString());
        }

        [TestMethod]
        public void Signature_Test()
        {
            var input = @"signature=""ISxp4HYhkGc83SsCC3zNZMVKBota7HqgWVg6KwaQVTQcUqt+UsWXPxB0XPhaYkqLnH3hJ+KVMdoEn3rbzcw8XZpjVt48o9OAKd0rsEZYkLoERnnFFhEw0GmVDEhdoU7gyoeOreWGsIca6Pf7TC0vGTtqez31zmvoeXvxHgqRhWQvlZM/ovFR2xN+vhmF7rZdkd6UaKOzy21K8B/Q84J7PWdbJ8i0rKieVPDIuTCy5B0iQpgs1TMaz6xKZR/KVzAr207m9Gkku2gnJ4YZHFuoa2ct5M5AtIPMPCsWTU8yaimTkPdNNezSOKV5a7T55HSvFeopLNcQKsWNMioKGpZP5hCIRKNk0Ekx0yDReE6xF/qliT7eSAGVJ/6sLQjBpBFMPKBNOrYTxueBJGtISjCZlxaIyTtJ1ErNuCrKHGjImpNvvJzTJOtu+vWnjTcUJL7N1Mw7PEreCZrNUyNuAldDWSMAFuD4HVA3+KZjpWCfjAbyelzVy2gs96CyE56o9FqJEaM5XVQhsMTpa8OSHdr2QZtKYw7Wng0d8vmbKEX1pdTVeEIhi4M9js39ZdzB4mb8JXSBE/GA6PoE5s+oH3+GoufzJYINCpk0Ulwo9g7HKm9NATnwEZZPq4NKto5mSYZKYRtqjZaa8lIALNhdvzv2+8+ifPLHlOigAUVqoupd9Aq=""";

            Assert.IsTrue(HeaderRegexes.HeaderSignature.IsMatch(input));
            var result = HeaderRegexes.HeaderSignature.Match(input);
            Assert.AreEqual("signature", result.Groups[1].ToString());
            Assert.AreEqual("ISxp4HYhkGc83SsCC3zNZMVKBota7HqgWVg6KwaQVTQcUqt+UsWXPxB0XPhaYkqLnH3hJ+KVMdoEn3rbzcw8XZpjVt48o9OAKd0rsEZYkLoERnnFFhEw0GmVDEhdoU7gyoeOreWGsIca6Pf7TC0vGTtqez31zmvoeXvxHgqRhWQvlZM/ovFR2xN+vhmF7rZdkd6UaKOzy21K8B/Q84J7PWdbJ8i0rKieVPDIuTCy5B0iQpgs1TMaz6xKZR/KVzAr207m9Gkku2gnJ4YZHFuoa2ct5M5AtIPMPCsWTU8yaimTkPdNNezSOKV5a7T55HSvFeopLNcQKsWNMioKGpZP5hCIRKNk0Ekx0yDReE6xF/qliT7eSAGVJ/6sLQjBpBFMPKBNOrYTxueBJGtISjCZlxaIyTtJ1ErNuCrKHGjImpNvvJzTJOtu+vWnjTcUJL7N1Mw7PEreCZrNUyNuAldDWSMAFuD4HVA3+KZjpWCfjAbyelzVy2gs96CyE56o9FqJEaM5XVQhsMTpa8OSHdr2QZtKYw7Wng0d8vmbKEX1pdTVeEIhi4M9js39ZdzB4mb8JXSBE/GA6PoE5s+oH3+GoufzJYINCpk0Ulwo9g7HKm9NATnwEZZPq4NKto5mSYZKYRtqjZaa8lIALNhdvzv2+8+ifPLHlOigAUVqoupd9Aq=", result.Groups[2].ToString());
        }
    }
}
