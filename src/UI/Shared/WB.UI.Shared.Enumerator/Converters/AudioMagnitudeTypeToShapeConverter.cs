using System;
using System.Globalization;
using MvvmCross.Platform.Converters;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class AudioMagnitudeTypeToShapeConverter : MvxValueConverter<MagnitudeType, int>
    {
        protected override int Convert(MagnitudeType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case MagnitudeType.Low:
                    return Resource.Drawable.audio_curcle_gray;
                case MagnitudeType.High:
                    return Resource.Drawable.audio_curcle_red;
                default:
                    return Resource.Drawable.audio_curcle_blue;
            }
        }
    }

    public class AudioMagnitudeTypeToDotConverter : MvxValueConverter<MagnitudeType, int>
    {
        protected override int Convert(MagnitudeType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case MagnitudeType.Low:
                    return Resource.Drawable.audio_dot_gray;
                case MagnitudeType.High:
                    return Resource.Drawable.audio_dot_red;
                default:
                    return Resource.Drawable.audio_dot_blue;
            }
        }
    }
}