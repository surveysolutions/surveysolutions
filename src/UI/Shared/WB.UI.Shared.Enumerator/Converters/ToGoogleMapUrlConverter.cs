using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class ToGoogleMapUrlConverter : MvxValueConverter<GpsLocation, string>
    {
        protected override string Convert(GpsLocation value, Type targetType, object parameter, CultureInfo culture)
        {
            bool showMap = (bool) parameter;
            if (value == null || !showMap)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=11&scale=2&size=590x380&markers=color:blue%7Clabel:O%7C{0},{1}&key=AIzaSyApLR-feMXRPfdDGjuC9NhlzH5udpuTgE4",
                value.Latitude, value.Longitude);
        }
    }
}