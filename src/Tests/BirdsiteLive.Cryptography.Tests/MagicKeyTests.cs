using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Frameworks;

namespace BirdsiteLive.Cryptography.Tests
{
    [TestClass]
    public class MagicKeyTests
    {
        [TestMethod]
        public void Test()
        {
            var g = MagicKey.Generate();
            var magicKey = new MagicKey(g.PrivateKey);

            Assert.IsNotNull(magicKey);
        }
    }
}