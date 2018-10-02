using System;
using Java.Security;
using Javax.Crypto;

namespace WB.UI.Shared.Enumerator.Activities
{
    /// <summary>
    /// Implementation of <see cref="ISecureStorage"/> using Android KeyStore.
    /// </summary>
    public class SecureStorage : ISecureStorage
    {
        private readonly KeyStore keyStore;
        private readonly KeyStore.PasswordProtection protection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureStorage"/> class.
        /// </summary>
        /// <param name="password">Password to use for encryption.</param>
        public SecureStorage(string password)
        {
            var passwordChars = password.ToCharArray();

            this.keyStore = KeyStore.GetInstance(KeyStore.DefaultType);
            this.protection = new KeyStore.PasswordProtection(passwordChars);

            this.keyStore.Load(null, passwordChars);
        }

        /// <summary>
        /// Stores data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="dataBytes">Data bytes to store.</param>
        public void Store(string key, byte[] dataBytes) 
            => this.keyStore.SetEntry(key, new KeyStore.SecretKeyEntry(new SecureData(dataBytes)), this.protection);

        /// <summary>
        /// Retrieves stored data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <returns>Byte array of stored data.</returns>
        public byte[] Retrieve(string key)
        {
            if (!(this.keyStore.GetEntry(key, this.protection) is KeyStore.SecretKeyEntry entry))
            {
                throw new Exception($"No entry found for key {key}.");
            }

            return entry.SecretKey.GetEncoded();
        }

        /// <summary>
        /// Deletes data.
        /// </summary>
        /// <param name="key">Key for the data to be deleted.</param>
        public void Delete(string key) => this.keyStore.DeleteEntry(key);

        /// <summary>
        /// Checks if the storage contains a key.
        /// </summary>
        /// <param name="key">The key to search.</param>
        /// <returns>True if the storage has the key, otherwise false.</returns>
        public bool Contains(string key) => this.keyStore.ContainsAlias(key);

        private class SecureData : Java.Lang.Object, ISecretKey
        {
            private const string Raw = "RAW";

            private readonly byte[] data;

            public SecureData(byte[] dataBytes)
            {
                this.data = dataBytes;
            }

            public string Algorithm => Raw;

            public string Format => Raw;

            public byte[] GetEncoded() => this.data;
        }
    }
}
