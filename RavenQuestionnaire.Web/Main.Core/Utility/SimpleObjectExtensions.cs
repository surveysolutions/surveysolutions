﻿using System;
using System.Linq;

namespace Main.Core.Utility
{
    public static class RepositoryKeysHelper
    {
        public static string GetVersionedKey(string id, long version)
        {
            return string.Format("{0}-{1}", id, version);
        }
    }

    /// <summary>
    /// The simple object extensions.
    /// </summary>
    public static class SimpleObjectExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The is integer.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsInteger(this string source)
        {
            int iSource;
            return Int32.TryParse(source, out iSource);
        }

        public static Guid Combine(this Guid x, Guid y)
        {
            byte[] a = x.ToByteArray();
            byte[] b = y.ToByteArray();

            return new Guid(BitConverter.GetBytes(BitConverter.ToUInt64(a, 0) ^ BitConverter.ToUInt64(b, 8))
                                        .Concat(BitConverter.GetBytes(BitConverter.ToUInt64(a, 8) ^ BitConverter.ToUInt64(b, 0))).ToArray());
        }

        public static Guid Combine(this Guid x, long y)
        {
            byte[] a = x.ToByteArray();
            byte[] b = BitConverter.GetBytes(y);

            return new Guid(BitConverter.GetBytes(BitConverter.ToUInt64(a, 0))
                                        .Concat(BitConverter.GetBytes(BitConverter.ToUInt64(a, 8) ^ BitConverter.ToUInt64(b, 0))).ToArray());
        }
        #endregion
    }
}