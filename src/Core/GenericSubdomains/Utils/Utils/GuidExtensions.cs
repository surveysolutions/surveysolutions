using System;
using System.Linq;
using System.Text;

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

        public static Guid Combine(this Guid x, long y)
        {
            byte[] a = x.ToByteArray();
            byte[] b = BitConverter.GetBytes(y);

            return new Guid(BitConverter.GetBytes(BitConverter.ToUInt64(a, 0))
                                        .Concat(BitConverter.GetBytes(BitConverter.ToUInt64(a, 8) ^ BitConverter.ToUInt64(b, 0))).ToArray());
        }

        public static Guid ToGuid(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) 
                return Guid.Empty;
            int chuncksCount = (int)Math.Ceiling((decimal)((value.Length) * sizeof(char))/16);
            int arraySize = chuncksCount*16;
            var data = new byte[arraySize];
            Buffer.BlockCopy(value.ToCharArray(), 0, data, 0, value.Length);

            var bytes16 = new byte[16];
            for (int i = 0; i < chuncksCount; i++)
            {
                byte[] part1 = BitConverter.GetBytes((BitConverter.ToUInt64(data, i*16) >> (i%2)*4) ^ BitConverter.ToUInt64(bytes16, 0));
                byte[] part2 = BitConverter.GetBytes((BitConverter.ToUInt64(data, i*16 + 8) >> (i%2)*4) ^ BitConverter.ToUInt64(bytes16, 8));
                bytes16 = part1.Concat(part2).ToArray();
            }

            return new Guid(bytes16);
        }
    }
}