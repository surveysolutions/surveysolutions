using System;
using System.Security.Cryptography;
using System.Text;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Activities
{
    public class RsaEncryptionService : IEncryptionService
    {
        private const string PublicKey = "publicKey";
        private const string PrivateKey = "privateKey";

        private readonly ISecureStorage secureStorage;

        public RsaEncryptionService(ISecureStorage secureStorage)
        {
            this.secureStorage = secureStorage;
        }

        public void GenerateKeys()
        {
            if (this.secureStorage.Contains(PublicKey) && this.secureStorage.Contains(PrivateKey)) return;

            var cryptoServiceProvider = new RSACryptoServiceProvider(2048);
            var privateKey = cryptoServiceProvider.ExportParameters(true);
            var publicKey = cryptoServiceProvider.ExportParameters(false);

            string publicKeyString = GetKeyString(publicKey);
            string privateKeyString = GetKeyString(privateKey);

            this.secureStorage.Store(PublicKey, Encoding.UTF8.GetBytes(publicKeyString));
            this.secureStorage.Store(PrivateKey, Encoding.UTF8.GetBytes(privateKeyString));
        }

        private static string GetKeyString(RSAParameters publicKey)
        {
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, publicKey);
            return stringWriter.ToString();
        }

        public string Encrypt(string textToEncrypt)
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    var publicKeyBytes = this.secureStorage.Retrieve(PublicKey);
                    var publicKeyString = Encoding.UTF8.GetString(publicKeyBytes);

                    rsa.FromXmlString(publicKeyString);
                    var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    return base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public string Decrypt(string textToDecrypt)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    var privateKeyBytes = this.secureStorage.Retrieve(PrivateKey);
                    var privateKeyString = Encoding.UTF8.GetString(privateKeyBytes);

                    // decrypting data with private key                    
                    rsa.FromXmlString(privateKeyString);

                    var resultBytes = Convert.FromBase64String(textToDecrypt);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
    }
}
