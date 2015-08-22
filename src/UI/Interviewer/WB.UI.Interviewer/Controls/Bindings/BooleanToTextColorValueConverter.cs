using System.Globalization;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;

namespace WB.UI.Interviewer.Controls.Bindings
{
    public class BooleanToTextColorValueConverter : MvxColorValueConverter
    {
        private static readonly MvxColor TrueBgColor = MvxColors.Black;
        private static readonly MvxColor FalseBgColor = MvxColors.Red;

        protected override MvxColor Convert(object value, object parameter, CultureInfo culture)
        {
            return (bool)value ? TrueBgColor : FalseBgColor;
        }
    }
}