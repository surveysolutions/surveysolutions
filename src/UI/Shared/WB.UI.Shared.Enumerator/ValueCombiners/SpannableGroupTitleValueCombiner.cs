using System.Collections.Generic;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Platform;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.ValueCombiners
{
    public class SpannableGroupTitleValueCombiner : BaseValueCombiner<SpannableString>
    {
        private ISubstitutionService SubstitutionService => Mvx.Resolve<ISubstitutionService>();

        protected override int ExpectedParamsCount => 3;

        protected override SpannableString GetValue(List<object> values)
        {
            bool isRoster = values[0].ConvertToBoolean();
            string groupTitle = values[1]?.ToString() ?? string.Empty;
            string rosterInstanceTitle = values[2]?.ToString() ?? string.Empty;

            if (!isRoster)
                return new SpannableString(groupTitle);

            var rosterTitle = this.SubstitutionService.GenerateRosterName(groupTitle, rosterInstanceTitle);
            var spannableRosterTitle = new SpannableString(rosterTitle);
            spannableRosterTitle.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), groupTitle.Length, rosterTitle.Length, SpanTypes.ExclusiveExclusive);

            return spannableRosterTitle;
        }
    }
}