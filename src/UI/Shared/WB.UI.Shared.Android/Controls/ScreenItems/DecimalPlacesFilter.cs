using System.Globalization;
using Android.Text;
using Java.Lang;
using Math = System.Math;

namespace WB.UI.Shared.Android.Controls.ScreenItems
{
    public class DecimalPlacesFilter : Java.Lang.Object, IInputFilter
    {
        private readonly int decimalPlacesCount;

        public DecimalPlacesFilter(int decimalPlacesCount)
        {
            this.decimalPlacesCount = decimalPlacesCount;
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            var text = dest.ToString() + source;

            var replacedAnswer = text.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
            decimal answer;
            if (!decimal.TryParse(replacedAnswer, out answer))
                return new SpannableString("");

            var roundedAnswer = Math.Round(answer, this.decimalPlacesCount);
            if (roundedAnswer != answer)
                return new SpannableString("");
            return null;
        }
    }
}