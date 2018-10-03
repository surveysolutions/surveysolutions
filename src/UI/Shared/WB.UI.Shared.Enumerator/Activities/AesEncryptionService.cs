using System;
using System.Security.Cryptography;
using System.Text;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Activities
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
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

            using (var aes = new AesCryptoServiceProvider())
            using (var cryptor = aes.CreateEncryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV)))
            {
                byte[] encryptedBytes = cryptor.TransformFinalBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public string Decrypt(string textToDecrypt)
        {
            using (var aes = new AesCryptoServiceProvider())
            using (var decryptor = aes.CreateDecryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV)))
            {
                var bytesToDecrypt = Convert.FromBase64String(textToDecrypt);

                byte[] decryptedBytes = decryptor.TransformFinalBlock(bytesToDecrypt, 0, bytesToDecrypt.Length);

                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
