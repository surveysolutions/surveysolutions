using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class ImageSourceConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return ImageSourceHelper.GetImageSource(value as byte[]);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class ToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? string.Empty : value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class StringFormatConverter : IValueConverter {
        public IValueConverter InnerConverter { get; set; }
        public object InnerConverterParameter { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(InnerConverter != null)
                value = InnerConverter.Convert(value, typeof(object), InnerConverterParameter, culture);
            return value == null ? string.Empty : string.Format((string)parameter, value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class ListFormatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string source = value as string;
            if(source == null) return string.Empty;
            string[] list = source.Split(',');
            string ret = string.Empty;
            foreach(string item in list)
                ret += string.Format("• {0}\r\n", item.Trim());
            return ret;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class AreEqualsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return object.Equals(value, parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
    public class ContentToControlConverter : IValueConverter {
        public bool AllowEmptyContent { get; set; }

        public ContentToControlConverter() {
            AllowEmptyContent = false;
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(!AllowEmptyContent && IsEmpty(value)) return null;
            DataTemplate contentTemplate = (DataTemplate)parameter;
            FrameworkElement control = (FrameworkElement)contentTemplate.LoadContent();
            control.DataContext = value;
            return control;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
        static bool IsEmpty(object content) {
            if(content == null) return true;
            string s = content as string;
            if(s != null && s.Length == 0) return true;
            return false;
        }
    }
    public class EmptyConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            System.Diagnostics.Debug.WriteLine("WOORKS!!!!");
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
    public class ComboBoxFormatConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? null : string.Format((string)parameter, value);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
    public class YearsCountConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            int? d = value as int?;
            if(d == null)
                return null;
            string years = d == 1 ? "year" : "years";
            return string.Format("{0} {1}", d, years);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
    public class DecimalRangeConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            decimal? i = value as decimal?;
            return i == null ? 0M : (decimal)i;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            decimal? i = value as decimal?;
            if(i != null) return (decimal)i;
            double? f = value as double?;
            return f == null ? 0M : (decimal)(double)f;
        }
    }
}
