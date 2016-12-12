using System.Collections.Generic;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class SpannableGroupTitleValueCombiner : BaseValueCombiner<SpannableString>
    {
        protected override int ExpectedParamsCount => 2;

        protected override SpannableString GetValue(List<object> values)
        {
            string groupTitle = values[0]?.ToString() ?? string.Empty;
            string rosterTitle = values[1]?.ToString() ?? string.Empty;

            var spannableTitle = new SpannableString(groupTitle);

            if (!string.IsNullOrEmpty(rosterTitle))
                spannableTitle.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), groupTitle.Length - rosterTitle.Length, groupTitle.Length, SpanTypes.ExclusiveExclusive);

            return spannableTitle;
        }
    }
}