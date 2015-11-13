using System.Globalization;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services
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

        public static decimal ToValue(string value)
        {
            return decimal.Parse(value, NumberFormat);
        }
    }
}