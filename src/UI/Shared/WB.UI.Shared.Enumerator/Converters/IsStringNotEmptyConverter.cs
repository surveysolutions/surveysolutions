using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class IsStringNotEmptyConverter : MvxValueConverter<string, bool>
    {
        protected override bool Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value);
        }

    }
}