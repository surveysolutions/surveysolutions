using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;

namespace WB.UI.Capi.Utils
{
    public class BackgroundColorValueConverter : MvxColorValueConverter
    {
        private static readonly MvxColor TrueBgColor = new MvxColor(0xFF000000);
        private static readonly MvxColor FalseBgColor = new MvxColor(0xFFFF0000);

        protected override MvxColor Convert(object value, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? TrueBgColor : FalseBgColor;
        }
    }
}