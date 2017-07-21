using System;
using System.Globalization;
using MvvmCross.Platform.Converters;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class AudioIntensityToShapeConverter : MvxValueConverter<float, int>
    {
        protected override int Convert(float value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resource.Drawable.audio_curcle_blue;
        }
    }

    public class AudioIntensityToDotConverter : MvxValueConverter<float, int>
    {
        protected override int Convert(float value, Type targetType, object parameter, CultureInfo culture)
        {
            return Resource.Drawable.audio_dot_blue;
        }
    }

    public class AudioIntensityToSizeConverter : MvxValueConverter<float, int>
    {
        protected override int Convert(float value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(value * 100);
        }
    }
}