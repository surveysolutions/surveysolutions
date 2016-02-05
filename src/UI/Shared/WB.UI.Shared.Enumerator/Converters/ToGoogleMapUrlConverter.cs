using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class ToGoogleMapUrlConverter : MvxValueConverter<GpsLocation, string>
    {
        protected override string Convert(GpsLocation value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "http://maps.googleapis.com/maps/api/staticmap?center={0},{1}&zoom=11&scale=2&size=640x380&markers=color:blue%7Clabel:O%7C{0},{1}",
                value.Latitude, value.Longitude);
        }
    }
}