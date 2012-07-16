using System;
using System.Globalization;

using System.Text;

namespace WinFormsSample.adept_part
{
    public class UFormat
    {
        public static decimal ToDecimal(string numStr)
        {
            //this is currently expecting either a "," or a "." separator
            var currentSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var numS = numStr;
            numS = numS.Replace(",", currentSeparator);
            numS = numS.Replace(".", currentSeparator);
            var result = Convert.ToDecimal(numS);
            return result;
        }
        public static string DecimalToString(decimal x)
        {
            //this is currently expecting either a "," or a "." separator
            var result = x.ToString();
            return result.Replace(",", ".");
        }
        public static string DecimalToString(decimal x, int digits)
        {
            var z = Decimal.Round(x, digits);
            return DecimalToString(z);
        }
        public static string DecimalSeparator
        {
            get
            {
                return (3.14.ToString().Substring(1, 1));
            }
        }
        public static string NiceSizeString(long x)
        {
            var fp = new FileSizeFormatProvider();
            return String.Format(fp, "{0:fs}", x);
        }

        public static string UUEncode(string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            var result = Convert.ToBase64String(b);
            return result;
        }
        public static string UUDecode(string s)
        {
            try
            {
                var b = Convert.FromBase64String(s);
                var result = Encoding.UTF8.GetString(b);
                return result;
            }
            catch
            {
                return String.Empty;
            }
        }

        /*public static string GetStringUTF8(string s)
        {
            var b = Encoding.UTF8.GetBytes(s);
            var result = Encoding.Default.GetString(b);
            return result;
        }*/

        public static bool IsNumber(string s)
        {
            double r;
            var result = double.TryParse(s, out r);
            return result;
        }
    }
}
