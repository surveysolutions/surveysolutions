using System;
using System.Globalization;
using MvvmCross.Converters;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class AudioNoiseTypeToShapeConverter : MvxValueConverter<NoiseType, int>
    {
        protected override int Convert(NoiseType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case NoiseType.Low:
                    return Resource.Drawable.audio_curcle_gray;
                case NoiseType.High:
                    return Resource.Drawable.audio_curcle_red;
                default:
                    return Resource.Drawable.audio_curcle_blue;
            }
        }
    }

    public class AudioNoiseTypeToDotConverter : MvxValueConverter<NoiseType, int>
    {
        protected override int Convert(NoiseType value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case NoiseType.Low:
                    return Resource.Drawable.audio_dot_gray;
                case NoiseType.High:
                    return Resource.Drawable.audio_dot_red;
                default:
                    return Resource.Drawable.audio_dot_blue;
            }
        }
    }
}
