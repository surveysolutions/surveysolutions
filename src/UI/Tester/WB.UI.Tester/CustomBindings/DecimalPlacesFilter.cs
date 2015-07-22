using System;
using System.Globalization;
using System.Linq;
using Android.Text;
using Java.Lang;
using Math = System.Math;

namespace WB.UI.Tester.CustomBindings
{
    public class DecimalPlacesFilter : Java.Lang.Object, IInputFilter
    {
        private readonly int decimalPlacesCount;

        private readonly string[] allowedStringValues = new string[] { CultureInfo.CurrentCulture.NumberFormat.NegativeSign };

        public DecimalPlacesFilter(int decimalPlacesCount)
        {
            this.decimalPlacesCount = Math.Max(decimalPlacesCount, 1);
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            var text = dest.ToString().Insert(dstart, source.ToString());
            var numberInInvariantCulture = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            decimal decimalNumber;
            if (decimal.TryParse(numberInInvariantCulture, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalNumber))
            {
                int countOfDecimalPlaces = BitConverter.GetBytes(decimal.GetBits(decimalNumber)[3])[2];
                if (countOfDecimalPlaces <= decimalPlacesCount)
                {
                    return null;
                }
                return new Java.Lang.String("");
            }

            if (allowedStringValues.Contains(numberInInvariantCulture)) 
                return null;
           
            return new Java.Lang.String("");
        }
    }
}