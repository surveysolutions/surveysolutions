using System;
using System.Globalization;
using System.IO;
using System.Text;
using Ionic.Zip;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Extensions
{
    public static class CustomExtensions
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static void PutFileEntry(this ZipOutputStream stream, string filename, byte[] content)
        {
            var entry = new ZipEntry() {AlternateEncoding = Encoding.Unicode};
            stream.PutNextEntry(filename);
            stream.Write(content, 0, content.Length);
        }

        public static void PutTextFileEntry(this ZipOutputStream stream, string filename, string text)
            => stream.PutFileEntry(filename, Encoding.UTF8.GetBytes(text ?? string.Empty));

        public static Guid AsGuid(this object source)
        {
            if (source == null)
                return Guid.Empty;
            return Guid.Parse(source.ToString());
        }

        public static int? InvertBooleableInt(this int? val, bool needValue)
        {
            return needValue && !val.ToBool() ? 1 : (int?)null;
        }

        public static bool ToBool(this int? val)
        {
            return val.HasValue && (val.Value == 1);
        }

        public static string ToUIString(this DateTime source)
        {
            return DateTime.Compare(source.ToUniversalTime(), DateTime.MinValue) == 0
                       ? GlobalHelper.EmptyString
                       : source.ToString(CultureInfo.InvariantCulture);
        }
    }
}
