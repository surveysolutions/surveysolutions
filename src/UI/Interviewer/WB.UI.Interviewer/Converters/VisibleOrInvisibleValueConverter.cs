using System;
using System.Globalization;
using Android.Views;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Platform;

namespace WB.UI.Interviewer.Converters
{
    public class VisibleOrInvisibleValueConverter : MvxValueConverter<bool, ViewStates>
    {
        protected override ViewStates Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            MvxTrace.Error("VisibleOrInvisibleValueConverter.Convert");
            return value ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}