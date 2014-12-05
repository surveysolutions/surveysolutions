using System;
using System.Linq;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class GuidExtensions
    {
        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }

        public static string FormatGuid(this Guid? guid)
        {
            if (!guid.HasValue)
            {
                return null;
            }

            return FormatGuid(guid.Value);
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
    }
}