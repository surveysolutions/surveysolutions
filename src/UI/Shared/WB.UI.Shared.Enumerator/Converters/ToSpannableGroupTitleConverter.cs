using System;
using System.Globalization;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using MvvmCross.Platform;
using MvvmCross.Platform.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class ToSpannableGroupTitleConverter : MvxValueConverter<string, SpannableString>
    {
        private ISubstitutionService SubstitutionService
        {
            get { return Mvx.Resolve<ISubstitutionService>(); }
        }

        protected override SpannableString Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            var groupViewModel = parameter as GroupViewModel;

            if (groupViewModel == null) return null;

            if (!groupViewModel.IsRoster)
                return new SpannableString(groupViewModel.Title);

            var rosterTitle = this.SubstitutionService.GenerateRosterName(groupViewModel.Title, value);
            var spannableRosterTitle = new SpannableString(rosterTitle);
            spannableRosterTitle.SetSpan(new StyleSpan(TypefaceStyle.BoldItalic), groupViewModel.Title.Length, rosterTitle.Length,
                SpanTypes.ExclusiveExclusive);

            return spannableRosterTitle;
        }
    }
}