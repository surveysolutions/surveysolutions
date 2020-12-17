using System;
using System.Text;

namespace StatData.Writers.Spss
{
    internal class SpssEncoder
    {
        // Encoding name as written in SPSS files
        internal static string GetSpssEncodingName()
        {
            switch (SpssConstants.EncodingCode)
            {
                case 1251:
                    return "windows-1251";
                case 1252:
                    return "windows-1252";
                case 1253:
                    return "windows-1253";
                case 1254:
                    return "windows-1254";
                case 1255:
                    return "windows-1255";
                case 1256:
                    return "windows-1256";
                case 1257:
                    return "windows-1257";
                case 1258:
                    return "windows-1258";
                case 65001:
                    return "UTF-8";
                default:
                    return "UNKNOWN??";
            }
        }

        internal static int GetEncodingMulti()
        {
            // trippling: http://www-01.ibm.com/support/docview.wss?uid=swg21502287

            var result = (SpssConstants.EncodingCode == 65001) ? 3 : 1;

            return result;
        }

        internal static byte[] GetStringBytes(string s)
        {
            if (String.IsNullOrEmpty(s)) return null;

            if (SpssConstants.EncodingCode == 1252)
                return Encoding.GetEncoding((int) SpssConstants.EncodingCode).GetBytes(s);
            else
                return Encoding.UTF8.GetBytes(s);
        }

        internal static byte[] GetStringBytes(string s, Int32 width)
        {
            // width determines mandatory width of the field

            var bytes = GetStringBytes(s);

            var result = new byte[width];
            for (var i = 0; i < width; i++)
            {
                if (bytes != null)
                {
                    result[i] = i < bytes.Length ? bytes[i] : SpssConstants.FillerByte;
                }
                else
                {
                    result[i] = SpssConstants.FillerByte;
                }
            }

            return result;
        }

        internal static byte[] GetStringBytesRounded(string s, byte multi)
        {
            // round upwards to multiples of multi
            var bytes = GetStringBytes(s);
            var len = (Int32) (Math.Ceiling((bytes.Length/(double) multi))*multi);

            var result = new byte[len];
            for (var i = 0; i < len; i++)
                result[i] = i < bytes.Length ? bytes[i] : SpssConstants.FillerByte;

            return result;
        }

        internal static byte[] GetStringBytes4(string s)
        {
            return GetStringBytesRounded(s, 4);
        }

        internal static byte[] GetStringBytes8(string s)
        {
            return GetStringBytesRounded(s, 8);
        }
    }
}
