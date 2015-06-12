using System.Globalization;
using System.Linq;

using Android.Text;

using Java.Lang;

using Math = System.Math;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class DecimalPlacesFilter : Java.Lang.Object, IInputFilter
    {
        private readonly int decimalPlacesCount;
        private readonly string[] allowedStringValues = new string[] { "-" };
        public DecimalPlacesFilter(int decimalPlacesCount)
        {
            this.decimalPlacesCount = decimalPlacesCount;
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            var text = dest.ToString() + source;
            var replacedAnswer = text.Replace(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
            decimal answer;
            if (!decimal.TryParse(replacedAnswer, NumberStyles.Number, CultureInfo.CurrentCulture, out answer))
            {
                if (this.allowedStringValues.Contains(replacedAnswer))
                    return null;
                return new SpannableString("");
            }

            var roundedAnswer = Math.Round(answer, this.decimalPlacesCount);
            if (roundedAnswer != answer)
                return new SpannableString("");
            return null;
        }
    }
}