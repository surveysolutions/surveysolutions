using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class NullableDecimalToStringConverter : MvxValueConverter<decimal?, string>
    {
        protected override string Convert(decimal? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.HasValue ? value.Value.ToString(culture) : string.Empty;
        }

        protected override decimal? ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal decimalValue;
            if (Decimal.TryParse(value, NumberStyles.Any, culture, out decimalValue))
            {
                return decimalValue;
            }
            return null;
        }
    }
}