using System;

namespace WB.Services.Export.Utils
{
    public class FileSizeUtils
    {
        public static string SizeSuffix(long value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int i = 0;
            decimal dValue = (decimal)value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", dValue, suffixes[i]);
        }

        public static double SizeInMegabytes(long value)
        {
            var oneMb = 1024 * 1024;
            double valueInMb = (double)value / oneMb;
            if (valueInMb < 1)
                return Math.Max(0.1, Math.Round(valueInMb, 1));
            if (valueInMb < 10)
                return Math.Round(valueInMb, 1);
            return Math.Round(valueInMb);
        }
    }
}