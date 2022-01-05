using System;
using System.Security.Cryptography;
using System.Text;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation
{
    public class RsaEncryptionService : IEncryptionService
    {
        private readonly ISecureStorage secureStorage;
        public const string PublicKey = "public-rsa";
        public const string PrivateKey = "private-rsa";

        public RsaEncryptionService(ISecureStorage secureStorage)
        {
            this.secureStorage = secureStorage;
        }

        public void GenerateKeys() => throw new NotSupportedException();

        public string Encrypt(string textToEncrypt)
            => string.IsNullOrEmpty(textToEncrypt)
                ? textToEncrypt
                : Convert.ToBase64String(this.Encrypt(Encoding.UTF8.GetBytes(textToEncrypt)));

        public string Decrypt(string textToDecrypt)
            => string.IsNullOrEmpty(textToDecrypt)
                ? textToDecrypt
                : Encoding.UTF8.GetString(this.Decrypt(Convert.FromBase64String(textToDecrypt)));

        public byte[] Encrypt(byte[] value)
        {
            if (!this.secureStorage.Contains(PublicKey))
                throw new Exception("RSA public key missing.");

            using (var rsaPublic = new RSACryptoServiceProvider(4096))
            {
                var publicKeyBytes = this.secureStorage.Retrieve(PublicKey);
                var publicKey = Encoding.UTF8.GetString(publicKeyBytes);

                RSACryptoServiceProviderExtensions.FromXmlString(rsaPublic, publicKey);

                return rsaPublic.Encrypt(value, false);
            }
        }

        public byte[] Decrypt(byte[] value)
        {
            if (!this.secureStorage.Contains(PrivateKey))
                throw new Exception("RSA private key is missing.");

            using (var rsaPrivate = new RSACryptoServiceProvider(4096))
            {
                var privateKeyBytes = this.secureStorage.Retrieve(PrivateKey);
                var privateKey = Encoding.UTF8.GetString(privateKeyBytes);

                RSACryptoServiceProviderExtensions.FromXmlString(rsaPrivate, privateKey);

                return rsaPrivate.Decrypt(value, false);
            }
        }
    }
}
