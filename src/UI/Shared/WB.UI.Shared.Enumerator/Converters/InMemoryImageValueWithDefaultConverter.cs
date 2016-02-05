using System;
using System.Globalization;
using Android.Graphics;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Droid.Platform;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class InMemoryImageValueWithDefaultConverter : MvxValueConverter<byte[], Bitmap>
    {
        protected override Bitmap Convert(byte[] value, Type targetType, object parameter, CultureInfo culture)
        {
            var options = new BitmapFactory.Options { InPurgeable = true };

            if (value != null)
                return BitmapFactory.DecodeByteArray(value, 0, value.Length, options);

            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var resources = mvxAndroidCurrentTopActivity.Activity.Resources;
            return BitmapFactory.DecodeResource(resources, Resource.Drawable.no_image_found, options);
        }
    }
}