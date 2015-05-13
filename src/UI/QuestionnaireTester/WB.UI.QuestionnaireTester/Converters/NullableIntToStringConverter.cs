using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class NullableIntToStringConverter : MvxValueConverter<int?, string>
    {
        protected override string Convert(int? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.HasValue ? value.Value.ToString(culture) : string.Empty;
        }

        protected override int? ConvertBack(string value, Type targetType, object parameter, CultureInfo culture)
        {
            int intValue;
            if (int.TryParse(value, out intValue))
            {
                return intValue;
            }
            return null; 
        }
    }
}