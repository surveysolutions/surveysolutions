// -----------------------------------------------------------------------
// <copyright file="RSACryptoService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Register
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
        RSAParameters GetPublicKey();

        /// <summary>
        /// Generate PrivateKey
        /// </summary>
        /// <returns>
        /// Return PrivateKey
        /// </returns>
        RSAParameters GetPrivateKey();
    }

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RSACryptoService : IRSACryptoService
    {
        /// <summary>
        /// Generic PublicKey
        /// </summary>
        /// <returns>
        /// Return PublicKey RSACrypt
        /// </returns>
        public RSAParameters GetPublicKey()
        {
            var rsa = new RSACryptoServiceProvider(2000);
            var publicKey = rsa.ExportParameters(false);
            return publicKey;
        }

        /// <summary>
        /// Generic PrivateKey
        /// </summary>
        /// <returns>
        /// Return PrivateKey RSACrypt
        /// </returns>
        public RSAParameters GetPrivateKey()
        {
            var rsa = new RSACryptoServiceProvider(2000);
            var privateKey = rsa.ExportParameters(true);
            return privateKey;
        }
    }
}
