// -----------------------------------------------------------------------
// <copyright file="RSACryptoService.cs" company="World Bank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.Services
{
    using System;
    using System.Collections;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Get Data in RSACryptoService
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

        /// <summary>
        /// Generate crypting data
        /// </summary>
        /// <param name="inputString">
        /// The input string.
        /// </param>
        /// <param name="dwKeySize">
        /// The dw key size.
        /// </param>
        /// <param name="xmlString">
        /// The xml string.
        /// </param>
        /// <returns>
        /// Return crypted data
        /// </returns>
        public string Encrypt(string inputString, int dwKeySize, string xmlString)
        {
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int keySize = dwKeySize / 8;
            byte[] bytes = Encoding.UTF32.GetBytes(inputString);
            int maxLength = keySize - 42;
            int dataLength = bytes.Length;
            int iterations = dataLength / maxLength;
            var stringBuilder = new StringBuilder();
            for (int i = 0; i <= iterations; i++)
            {
                byte[] tempBytes = new byte[(dataLength - maxLength * i > maxLength) ? maxLength : dataLength - maxLength * i];
                Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0, tempBytes.Length);
                byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt(tempBytes, true);
                stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generate decrypting data
        /// </summary>
        /// <param name="inputString">
        /// The input string.
        /// </param>
        /// <param name="dwKeySize">
        /// The dw key size.
        /// </param>
        /// <param name="xmlString">
        /// The xml string.
        /// </param>
        /// <returns>
        /// Return decrypted data
        /// </returns>
        public string Decrypt(string inputString, int dwKeySize, string xmlString)
        {
            var rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
            rsaCryptoServiceProvider.FromXmlString(xmlString);
            int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ?
              (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
            int iterations = inputString.Length / base64BlockSize;
            var arrayList = new ArrayList();
            for (int i = 0; i < iterations; i++)
            {
                byte[] encryptedBytes = Convert.FromBase64String(inputString.Substring(base64BlockSize * i, base64BlockSize));
                arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(encryptedBytes, true));
            }

            return Encoding.UTF32.GetString(arrayList.ToArray(Type.GetType("System.Byte")) as byte[]);
        }
    }
}
