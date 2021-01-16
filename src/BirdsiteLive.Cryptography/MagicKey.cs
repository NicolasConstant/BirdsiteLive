using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace BirdsiteLive.Cryptography
{
    public class MagicKey
    {
        //public class WebfingerLink
        //{
        //    public string rel { get; set; }
        //    public string type { get; set; }
        //    public string href { get; set; }
        //    public string template { get; set; }
        //}

        //public class WebfingerResult
        //{
        //    public string subject { get; set; }
        //    public List<string> aliases { get; set; }
        //    public List<WebfingerLink> links { get; set; }
        //}

        private string[] _parts;
        private RSA _rsa;

        private static byte[] _decodeBase64Url(string data)
        {
            return Convert.FromBase64String(data.Replace('-', '+').Replace('_', '/'));
        }

        private static string _encodeBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_');
        }

        private class RSAKeyParms
        {
            public byte[] D;
            public byte[] DP;
            public byte[] DQ;
            public byte[] Exponent;
            public byte[] InverseQ;
            public byte[] Modulus;
            public byte[] P;
            public byte[] Q;

            public static RSAKeyParms From(RSAParameters parms)
            {
                var a = new RSAKeyParms();
                a.D = parms.D;
                a.DP = parms.DP;
                a.DQ = parms.DQ;
                a.Exponent = parms.Exponent;
                a.InverseQ = parms.InverseQ;
                a.Modulus = parms.Modulus;
                a.P = parms.P;
                a.Q = parms.Q;
                return a;
            }

            public RSAParameters Make()
            {
                var a = new RSAParameters();
                a.D = D;
                a.DP = DP;
                a.DQ = DQ;
                a.Exponent = Exponent;
                a.InverseQ = InverseQ;
                a.Modulus = Modulus;
                a.P = P;
                a.Q = Q;
                return a;
            }
        }

        public MagicKey(string key)
        {
            if (key[0] == '{')
            {
                _rsa = RSA.Create();
                _rsa.ImportParameters(JsonConvert.DeserializeObject<RSAKeyParms>(key).Make());
            }
            else
            {
                _parts = key.Split('.');
                if (_parts[0] != "RSA") throw new Exception("Unknown magic key!");

                var rsaParams = new RSAParameters();
                rsaParams.Modulus = _decodeBase64Url(_parts[1]);
                rsaParams.Exponent = _decodeBase64Url(_parts[2]);

                _rsa = RSA.Create();
                _rsa.ImportParameters(rsaParams);
            }
        }

        public static MagicKey Generate()
        {
            var rsa = RSA.Create();
            rsa.KeySize = 2048;

            return new MagicKey(JsonConvert.SerializeObject(RSAKeyParms.From(rsa.ExportParameters(true))));
        }

        public byte[] BuildSignedData(string data, string dataType, string encoding, string algorithm)
        {
            var sig = data + "." + _encodeBase64Url(Encoding.UTF8.GetBytes(dataType)) + "." + _encodeBase64Url(Encoding.UTF8.GetBytes(encoding)) + "." + _encodeBase64Url(Encoding.UTF8.GetBytes(algorithm));
            return Encoding.UTF8.GetBytes(sig);
        }

        public bool Verify(string signature, byte[] data)
        {
            return _rsa.VerifyData(data, _decodeBase64Url(signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public byte[] Sign(byte[] data)
        {
            return _rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        public string AsPEM
        {
            get
            {
                var data = ASN1.FromRSA(_rsa);
                var baseData = Convert.ToBase64String(data);
                var builder = new StringBuilder(baseData);
                for (int i = 72; i < builder.Length; i += 73)
                    builder.Insert(i, "\n");

                builder.Insert(0, "-----BEGIN PUBLIC KEY-----\n");
                builder.Append("\n-----END PUBLIC KEY-----");

                return builder.ToString();
            }
        }

        public string PrivateKey
        {
            get { return JsonConvert.SerializeObject(RSAKeyParms.From(_rsa.ExportParameters(true))); }
        }

        public string PublicKey
        {
            get
            {
                var parms = _rsa.ExportParameters(false);

                return string.Join(".", "RSA", _encodeBase64Url(parms.Modulus), _encodeBase64Url(parms.Exponent));
            }
        }
    }
}