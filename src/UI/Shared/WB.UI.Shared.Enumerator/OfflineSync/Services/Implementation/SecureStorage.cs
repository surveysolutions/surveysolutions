using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Java.IO;
using Java.Security;
using Javax.Crypto;
using MvvmCross.Platforms.Android;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Implementation
{
    /// <summary>
    /// Implementation of <see cref="ISecureStorage"/> using Android KeyStore.
    /// </summary>
    public class SecureStorage : ISecureStorage
    {
        private readonly IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity;
        private readonly KeyStore keyStore;
        private readonly Dictionary<string, byte[]> inMemoryKeyStorage = new Dictionary<string, byte[]>();

        static readonly object keyStoreFileLock = new object();
        static string keyStoreFileName = "keystore.dat";

        public SecureStorage(IMvxAndroidCurrentTopActivity mvxAndroidCurrentTopActivity)
        {
            this.mvxAndroidCurrentTopActivity = mvxAndroidCurrentTopActivity;

            this.keyStore = KeyStore.GetInstance(KeyStore.DefaultType);

            try
            {
                lock (keyStoreFileLock)
                {
                    using (var keyStoreFile = this.mvxAndroidCurrentTopActivity.Activity.OpenFileInput(keyStoreFileName))
                    {
                        this.keyStore.Load(keyStoreFile, null);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                this.LoadEmptyKeyStore(null);
            }
        }

        private void Save()
        {
            lock (keyStoreFileLock)
            {
                using (var keyStoreFile = this.mvxAndroidCurrentTopActivity.Activity.OpenFileOutput(keyStoreFileName, FileCreationMode.Private))
                {
                    this.keyStore.Store(keyStoreFile, null);
                }
            }
        }

        /// <summary>
        /// Stores data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <param name="dataBytes">Data bytes to store.</param>
        public void Store(string key, byte[] dataBytes)
        {
            this.keyStore.SetEntry(key, new KeyStore.SecretKeyEntry(new SecureData(dataBytes)), null);
            Save();
        }

        /// <summary>
        /// Retrieves stored data.
        /// </summary>
        /// <param name="key">Key for the data.</param>
        /// <returns>Byte array of stored data.</returns>
        public byte[] Retrieve(string key)
        {
            if (inMemoryKeyStorage.ContainsKey(key)) return inMemoryKeyStorage[key];
            if (!(this.keyStore.GetEntry(key, null) is KeyStore.SecretKeyEntry entry))
            {
                throw new Exception($"No entry found for key {key}.");
            }

            this.inMemoryKeyStorage[key] = entry.SecretKey.GetEncoded();
            return inMemoryKeyStorage[key];
        }

        /// <summary>
        /// Deletes data.
        /// </summary>
        /// <param name="key">Key for the data to be deleted.</param>
        public void Delete(string key)
        {
            this.keyStore.DeleteEntry(key);
            Save();
        }

        /// <summary>
        /// Checks if the storage contains a key.
        /// </summary>
        /// <param name="key">The key to search.</param>
        /// <returns>True if the storage has the key, otherwise false.</returns>
        public bool Contains(string key) => this.keyStore.ContainsAlias(key);

        static IntPtr id_load_Ljava_io_InputStream_arrayC;

        /// <summary>
        /// Work around Bug https://bugzilla.xamarin.com/show_bug.cgi?id=6766
        /// </summary>
        void LoadEmptyKeyStore(char[] password)
        {
            if (id_load_Ljava_io_InputStream_arrayC == IntPtr.Zero)
            {
                id_load_Ljava_io_InputStream_arrayC = JNIEnv.GetMethodID(keyStore.Class.Handle, "load", "(Ljava/io/InputStream;[C)V");
            }
            IntPtr intPtr = IntPtr.Zero;
            IntPtr intPtr2 = JNIEnv.NewArray(password);
            JNIEnv.CallVoidMethod(keyStore.Handle, id_load_Ljava_io_InputStream_arrayC, new JValue[]
            {
                new JValue (intPtr),
                new JValue (intPtr2)
            });
            JNIEnv.DeleteLocalRef(intPtr);
            if (password != null)
            {
                JNIEnv.CopyArray(intPtr2, password);
                JNIEnv.DeleteLocalRef(intPtr2);
            }
        }

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
