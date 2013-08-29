namespace Main.Core.Utility
{
    using System;
    using System.Linq;

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

        #endregion
    }
}