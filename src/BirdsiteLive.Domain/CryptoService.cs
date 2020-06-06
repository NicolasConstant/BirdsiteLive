using BirdsiteLive.Domain.Factories;

namespace BirdsiteLive.Domain
{
    public interface ICryptoService
    {
        string GetUserPem(string id);
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
    }
}