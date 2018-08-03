using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class EnumToStringConverter : MvxValueConverter<Enum, string>
    {
        protected override string Convert(Enum value, Type targetType, object parameter, CultureInfo culture) => value.ToString();
    }
}
