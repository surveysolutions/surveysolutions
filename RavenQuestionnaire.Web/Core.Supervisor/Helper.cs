// -----------------------------------------------------------------------
// <copyright file="Helper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Converts string into the md5 hash and coverts bytes to Guid
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The guid
        /// </returns>
        public static Guid StringToGuid(string key)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(key));
            return new Guid(data);
        }
    }
}
