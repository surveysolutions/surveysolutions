using System;
using System.Globalization;
using Android.Views;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class VisibleOrInvisibleValueConverter : MvxValueConverter<bool, ViewStates>
    {
        protected override ViewStates Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ? ViewStates.Visible : ViewStates.Invisible;
        }
    }
}