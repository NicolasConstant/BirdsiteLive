using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Cryptography.Tests
{
    [TestClass]
    public class MagicKeyTests
    {
        [TestMethod]
        public async Task Test()
        {
            var g = MagicKey.Generate();

            var magicKey = new MagicKey(g.PrivateKey);


        }
    }
}