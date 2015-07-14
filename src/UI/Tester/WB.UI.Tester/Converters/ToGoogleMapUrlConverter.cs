using System;
using System.Globalization;
using Cirrious.CrossCore.Converters;
using Cirrious.MvvmCross.Plugins.Location;

namespace WB.UI.Tester.Converters
{
    public class ToGoogleMapUrlConverter : MvxValueConverter<MvxCoordinates, string>
    {
        protected override string Convert(MvxCoordinates value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return string.Format(
                "http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=11&scale=2&size=640x380&markers=color:blue%7Clabel:O%7C{0},{1}",
                value.Latitude, value.Longitude);
        }
    }
}