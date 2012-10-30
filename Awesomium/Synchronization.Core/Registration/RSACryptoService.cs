// -----------------------------------------------------------------------
// <copyright file="RSACryptoService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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
    /// TODO: Update summary.
    /// </summary>
    public class RSACryptoService:IRSACryptoService
    {
        /// <summary>
        /// Generic PublicKey
        /// </summary>
        /// <returns>
        /// Return PublicKey RSACrypt
        /// </returns>
        public RSAParameters GetPublicKey(string key)
        {
            var cp = new CspParameters {KeyContainerName = key, Flags = CspProviderFlags.UseExistingKey};
            var rsa = new RSACryptoServiceProvider(2000, cp);
            var publicKey = rsa.ExportParameters(false);
            return publicKey;
        }

        /// <summary>
        /// Generic PrivateKey
        /// </summary>
        /// <returns>
        /// Return PrivateKey RSACrypt
        /// </returns>
        public RSAParameters GetPrivateKey(string key)
        {
            var cp = new CspParameters { KeyContainerName = key, Flags = CspProviderFlags.UseExistingKey };
            var rsa = new RSACryptoServiceProvider(2000, cp);
            var privateKey = rsa.ExportParameters(true);
            return privateKey;
        }
    }
}
