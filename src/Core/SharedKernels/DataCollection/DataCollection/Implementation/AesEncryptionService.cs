using System;
using System.Security.Cryptography;
using System.Text;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation
{
    public class AesEncryptionService : IEncryptionService, IDisposable
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

            using var aesCreate = Aes.Create();
            aesCreate.GenerateIV();
            this.secureStorage.Store(IV, aes.IV);

            aesCreate.GenerateKey();
            this.secureStorage.Store(Key, aes.Key);
        }

        public string Encrypt(string textToEncrypt)
            => string.IsNullOrEmpty(textToEncrypt)
                ? textToEncrypt
                : Convert.ToBase64String(this.Encrypt(Encoding.UTF8.GetBytes(textToEncrypt)));

        public string Decrypt(string textToDecrypt)
            => string.IsNullOrEmpty(textToDecrypt)
                ? textToDecrypt
                : Encoding.UTF8.GetString(this.Decrypt(Convert.FromBase64String(textToDecrypt)));

        private ICryptoTransform encryptor = null;
        private ICryptoTransform decryptor = null;
        private Aes aes = null;
        private readonly object syncLock = new object();
        
        private void EnsureInitialized()
        {
            if (encryptor != null) return;
            
            lock (syncLock)
            {
                if (!this.secureStorage.Contains(Key) || !this.secureStorage.Contains(IV))
                    throw new Exception("Key or Initialization vector is missing.");
                                
                aes = Aes.Create();
                encryptor = aes.CreateEncryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV));
                decryptor = aes.CreateDecryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV));
            }
        }
        public byte[] Encrypt(byte[] value)
        {
            EnsureInitialized();

            if (!encryptor.CanReuseTransform)
                return aes.CreateEncryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV))
                    .TransformFinalBlock(value, 0, value.Length);
            
            lock (syncLock)
            {
                return encryptor.TransformFinalBlock(value, 0, value.Length);
            }
        }

        public byte[] Decrypt(byte[] value)
        {
            EnsureInitialized();
            if (!decryptor.CanReuseTransform)
                return aes.CreateDecryptor(this.secureStorage.Retrieve(Key), this.secureStorage.Retrieve(IV))
                    .TransformFinalBlock(value, 0, value.Length);
            
            lock (syncLock)
            {
                return decryptor.TransformFinalBlock(value, 0, value.Length);
            }
        }

        public void Dispose()
        {
            encryptor?.Dispose();
            decryptor?.Dispose();
            aes?.Dispose();
        }
    }
}
