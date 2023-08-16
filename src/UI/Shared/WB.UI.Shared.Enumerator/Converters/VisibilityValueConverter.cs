using System.Globalization;
using Android.Views;
using MvvmCross.Base;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters;

public class VisibilityValueConverter : MvxValueConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = value.ConvertToBooleanCore();
        bool hide = parameter.ConvertToBooleanCore();

        if (!visible)
        {
            return hide ? ViewStates.Invisible : ViewStates.Gone;
        }

        return ViewStates.Visible;
    }
}