using System;
using System.Globalization;
using MvvmCross.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class IsNullableHavingValueConverter : MvxValueConverter<int?, bool>
    {
        protected override bool Convert(int? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.HasValue;
        }

    }
}
