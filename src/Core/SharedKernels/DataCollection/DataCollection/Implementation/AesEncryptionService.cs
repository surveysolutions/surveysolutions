using System;
using System.Security.Cryptography;
using System.Text;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation
{
    public class AesEncryptionService : IEncryptionService
    {
        private const string Key = "key";
        private const string IV = "iv";

        private readonly ISecureStorage secureStorage;

        public AesEncryptionService(ISecureStorage secureStorage)
        {
            this.secureStorage = secureStorage;
        }

        public void GenerateKeys()
        {
            if (this.secureStorage.Contains(Key) && this.secureStorage.Contains(IV)) return;

            using (var aes = new AesCryptoServiceProvider())
            {
                aes.GenerateIV();
                this.secureStorage.Store(IV, aes.IV);

                aes.GenerateKey();
                this.secureStorage.Store(Key, aes.Key);
            }
        }

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
            if (!this.secureStorage.Contains(Key) || !this.secureStorage.Contains(IV))
                throw new Exception("Key or Initialization vector is missing.");

            using (var aes = new AesCryptoServiceProvider())
            using (var cryptor = aes.CreateEncryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV)))
            {
                return cryptor.TransformFinalBlock(value, 0, value.Length);
            }
        }

        public byte[] Decrypt(byte[] value)
        {
            if (!this.secureStorage.Contains(Key) || !this.secureStorage.Contains(IV))
                throw new Exception("Key or Initialization vector is missing.");

            using (var aes = new AesCryptoServiceProvider())
            using (var decryptor = aes.CreateDecryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV)))
            {
                return decryptor.TransformFinalBlock(value, 0, value.Length);
            }
        }
    }
}
