using System;
using System.Globalization;
using System.Windows.Data;

namespace CapiDataGenerator
{
    class ElapsedTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan) value).ToString(@"dd\.hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
