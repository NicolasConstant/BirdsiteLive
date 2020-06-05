using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.Cryptography.Tests
{
    [TestClass]
    public class RsaGeneratorTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var rsaGen = new RsaGenerator();
            var rsa = rsaGen.GetRsa();
        }
    }
}
