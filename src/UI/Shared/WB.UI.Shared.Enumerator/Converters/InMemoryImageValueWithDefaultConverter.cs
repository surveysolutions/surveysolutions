using System;
using System.Globalization;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.Widget;
using MvvmCross.Converters;
using MvvmCross.Platforms.Android;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.UI.Shared.Enumerator.Converters
{
    public class InMemoryImageValueWithDefaultConverter : MvxValueConverter<byte[], Bitmap>
    {
        private static Bitmap fallbackBitmap;

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
            return BitmapFromVectorDrawable(mvxAndroidCurrentTopActivity.Activity, Resource.Drawable.img_placeholder);
        }

        private static Bitmap BitmapFromVectorDrawable(Context context, int drawableId) 
        {
            if (fallbackBitmap == null)
            {
                var drawable = AppCompatDrawableManager.Get().GetDrawable(context, drawableId);
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    drawable = DrawableCompat.Wrap(drawable).Mutate();
                }

                Bitmap bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth,
                    drawable.IntrinsicHeight, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);

                fallbackBitmap = bitmap;
            }

            return fallbackBitmap;
        }
    }
}
