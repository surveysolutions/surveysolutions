using System;
using System.Globalization;
using Android.Graphics;
using MvvmCross.Platform.Converters;
using MvvmCross.Platform.Droid.Platform;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class InMemoryImageValueWithDefaultConverter : MvxValueConverter<byte[], Bitmap>
    {
        protected override Bitmap Convert(byte[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var options = new BitmapFactory.Options { InPurgeable = true };
                var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, options);
                if (bitmap != null)
                    return bitmap;
            }

            var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
            var resources = mvxAndroidCurrentTopActivity.Activity.Resources;
            var noImageOptions = new BitmapFactory.Options { InPurgeable = true };
            return BitmapFactory.DecodeResource(resources, Resource.Drawable.no_image_found, noImageOptions);
        }
    }
}