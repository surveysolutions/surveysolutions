using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Android.Text;
using Java.Lang;
using Math = System.Math;

namespace WB.UI.QuestionnaireTester.CustomBindings
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
            decimal answer;
            if (decimal.TryParse(numberInInvariantCulture, NumberStyles.Number, CultureInfo.InvariantCulture, out answer))
            {
                Regex decimalRegex = new Regex(@"^[-+]?\d+([.,]\d{0," + decimalPlacesCount + @"})?$");
                
                if (decimalRegex.IsMatch(numberInInvariantCulture))
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