// -----------------------------------------------------------------------
// <copyright file="RSACryptoService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Synchronization.Core.Registration
{
    using System.Security.Cryptography;

    public interface IRSACryptoService
    {
        /// <summary>
        /// Generate PublicKey
        /// </summary>
        /// <returns>
        /// Return PrivateKey
        /// </returns>
        RSAParameters GetPublicKey(string key);

        /// <summary>
        /// Generate PrivateKey
        /// </summary>
        /// <returns>
        /// Return PrivateKey
        /// </returns>
        RSAParameters GetPrivateKey(string key);
    }

    /// <summary>
    /// Performs generation assymetric key pair or reading it from machine level container 
    /// </summary>
    public class RSACryptoService : IRSACryptoService
    {
        private readonly int KeySize = 1024;

        private RSACryptoServiceProvider InstantiateProvider(string keyContainerName, bool newKeys)
        {
            var param = new CspParameters
            {
                KeyContainerName = keyContainerName,
                Flags = newKeys ? 
                        CspProviderFlags.UseMachineKeyStore : 
                        CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,

            };

            try
            {
                return new RSACryptoServiceProvider(KeySize, param);
            }
            catch
            {
                return null;
            }
        }

        private RSAParameters AcceptKey(string keyContainerName, bool includePrivate)
        {
            var rsa = InstantiateProvider(keyContainerName, false);
            if (rsa == null)
                rsa = InstantiateProvider(keyContainerName, true);

            return rsa.ExportParameters(includePrivate);
        }

        /// <summary>
        /// Reads assymetric key pair from named container or gnerates new one if container doesn't contain the key
        /// </summary>
        /// <returns>
        /// RSA PublicKey
        /// </returns>
        public RSAParameters GetPublicKey(string keyContainerName)
        {
            return AcceptKey(keyContainerName, false);
        }

        /// <summary>
        /// Reads assymetric key pair from named container or gnerates new one if container doesn't contain the key
        /// </summary>
        /// <returns>
        /// RSA PrivateKey
        /// </returns>
        public RSAParameters GetPrivateKey(string keyContainerName)
        {
            return AcceptKey(keyContainerName, true);
        }
    }
}
