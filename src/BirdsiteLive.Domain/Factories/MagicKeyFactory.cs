using System.IO;
using BirdsiteLive.Cryptography;

namespace BirdsiteLive.Domain.Factories
{
    public interface IMagicKeyFactory
    {
        MagicKey GetMagicKey();
    }

    public class MagicKeyFactory : IMagicKeyFactory
    {
        private const string Path = "key.json";
        private static MagicKey _magicKey;

        #region Ctor
        public MagicKeyFactory()
        {
            
        }
        #endregion

        public MagicKey GetMagicKey()
        {
            //Cached key
            if (_magicKey != null) return _magicKey;

            //Generate key if needed
            if (!File.Exists(Path))
            {
                var key = MagicKey.Generate();
                File.WriteAllText(Path, key.PrivateKey);
            }

            //Load and return key
            var serializedKey = File.ReadAllText(Path);
            _magicKey = new MagicKey(serializedKey);
            return _magicKey;
        }
    }
}