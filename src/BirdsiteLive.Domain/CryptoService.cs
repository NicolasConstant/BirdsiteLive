using System;
using System.Text;
using BirdsiteLive.Domain.Factories;

namespace BirdsiteLive.Domain
{
    public interface ICryptoService
    {
        string GetUserPem(string id);
        string SignAndGetSignatureHeader(DateTime date, string actor, string host);
    }

    public class CryptoService : ICryptoService
    {
        private readonly IMagicKeyFactory _magicKeyFactory;

        #region Ctor
        public CryptoService(IMagicKeyFactory magicKeyFactory)
        {
            _magicKeyFactory = magicKeyFactory;
        }
        #endregion

        public string GetUserPem(string id)
        {
            return _magicKeyFactory.GetMagicKey().AsPEM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actor">in the form of https://domain.io/actor</param>
        /// <param name="host">in the form of domain.io</param>
        /// <returns></returns>
        public string SignAndGetSignatureHeader(DateTime date, string actor, string targethost)
        {
            var httpDate = date.ToString("r");

            var signedString = $"(request-target): post /inbox\nhost: {targethost}\ndate: {httpDate}";
            var signedStringBytes = Encoding.UTF8.GetBytes(signedString);
            var signature = _magicKeyFactory.GetMagicKey().Sign(signedStringBytes);
            var sig64 = Convert.ToBase64String(signature);

            var header = "keyId=\"" + actor + "\",headers=\"(request-target) host date\",signature=\"" + sig64 + "\"";
            return header;
        }
    }
}