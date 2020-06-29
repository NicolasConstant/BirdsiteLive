//using System.Security.Cryptography;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using MyProject.Data.Encryption;

//namespace BirdsiteLive.Cryptography.Tests
//{
//    [TestClass]
//    public class RsaKeysTests
//    {
//        [TestMethod]
//        public void TestMethod1()
//        {
//            var rsa = RSA.Create();

//            var cspParams = new CspParameters();
//            cspParams.ProviderType = 1; // PROV_RSA_FULL
//            cspParams.Flags = CspProviderFlags.CreateEphemeralKey;
//            var rsaProvider = new RSACryptoServiceProvider(2048, cspParams);

//            var rsaPublicKey = RSAKeys.ExportPublicKey(rsaProvider);
//            var rsaPrivateKey = RSAKeys.ExportPrivateKey(rsaProvider);

//            //rsaProvider.

//            var pem = RSAKeys.ImportPublicKey(rsaPrivateKey);
//        }

//        [TestMethod]
//        public void TestMethod2()
//        {

//        }
//    }
//}