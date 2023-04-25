using System.Globalization;
using Android.Views;
using MvvmCross.Base;
using MvvmCross.Converters;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.Converters;

public class InvertedVisibilityValueConverter : MvxValueConverter
{
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool visible = value.ConvertToBooleanCore();
        bool hide = parameter.ConvertToBooleanCore();

        if (!visible)
            return ViewStates.Visible;

        return hide ? ViewStates.Invisible : ViewStates.Gone;
    }
}