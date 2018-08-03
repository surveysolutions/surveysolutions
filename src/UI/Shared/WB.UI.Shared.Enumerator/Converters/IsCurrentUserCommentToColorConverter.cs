using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class IsCurrentUserCommentToColorConverter : MvxValueConverter<bool, int>
    {
        protected override int Convert(bool value, Type targetType, object parameter, CultureInfo culture)
            => value ? Resource.Color.comment_from_interviewer : Resource.Color.comment_from_authority;
    }
}