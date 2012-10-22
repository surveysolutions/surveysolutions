// -----------------------------------------------------------------------
// <copyright file="IRSACryptoService.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Services
{
    using System.Security.Cryptography;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IRSACryptoService
    {
        RSAParameters GetPublicKey();

        RSAParameters GetPrivateKey();

        string Encrypt(string inputString, int dwKeySize, string xmlString);

        string Decrypt(string inputString, int dwKeySize, string xmlString);
    }
}
