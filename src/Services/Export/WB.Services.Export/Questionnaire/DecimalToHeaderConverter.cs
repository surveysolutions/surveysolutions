using System.Globalization;

namespace WB.Services.Export.Questionnaire
{
    public static class DecimalToHeaderConverter
    {
        private static readonly NumberFormatInfo NumberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = "_",
            NegativeSign = "n"
        };


        public static string ToHeader(decimal value)
        {
            return value.ToString(NumberFormat);
        }

        public static decimal? ToValue(string value)
        {
            decimal result;

            if (decimal.TryParse(value, NumberStyles.Any, NumberFormat, out result))
            {
                return result;
            }

            return null;
        }
    }
}