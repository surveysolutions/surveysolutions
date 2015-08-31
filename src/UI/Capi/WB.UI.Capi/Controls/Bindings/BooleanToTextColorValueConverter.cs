using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;

namespace WB.UI.Capi.Utils
{
    public class BooleanToTextColorValueConverter : MvxColorValueConverter
    {
        private static readonly MvxColor TrueBgColor = MvxColors.Black;
        private static readonly MvxColor FalseBgColor = MvxColors.Red;

        protected override MvxColor Convert(object value, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? TrueBgColor : FalseBgColor;
        }
    }
}