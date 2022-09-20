using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ImageViewBitmapWithFallbackBinding : BaseBinding<ImageView, byte[]>
{
    public ImageViewBitmapWithFallbackBinding(ImageView androidControl) : base(androidControl)
    {
    }

    protected override void SetValueToView(ImageView control, byte[] value)
    {
        if (value != null)
        {
            var displayMetrics = GetDisplayMetrics();
            //var minSize = Math.Min(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
            var displayMinSize = Math.Min(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
            var controlMinSize = Math.Min(control.MaxHeight, control.MaxWidth);
            var minSize = Math.Min(displayMinSize, controlMinSize);

            // Calculate inSampleSize
            BitmapFactory.Options boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true, InPurgeable = true };
            using (BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions)) // To determine actual image size
            {
                int sampleSize = CalculateInSampleSize(boundsOptions, minSize, minSize);

                var bitmapOptions = new BitmapFactory.Options { InSampleSize = sampleSize, InPurgeable = true };
                using (var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions))
                {
                    if (bitmap != null)
                    {
                        SetupImageView(control, displayMetrics, boundsOptions);
                        control.Drawable?.Dispose();
                        control.SetImageBitmap(bitmap);
                    }
                    else
                    {
                        this.SetDefaultImage(control);
                    }
                }
            }
        }
        else
        {
            this.SetDefaultImage(control);
        }
    }

    protected virtual void SetDefaultImage(ImageView control)
    {
        control.SetImageResource(Resource.Drawable.img_placeholder);
    }

    protected virtual void SetupImageView(ImageView control, DisplayMetrics displayMetrics, BitmapFactory.Options boundsOptions)
    {
        
    }

    private static DisplayMetrics GetDisplayMetrics()
    {
        var defaultDisplay = Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay;
        DisplayMetrics displayMetrics = new DisplayMetrics();
        defaultDisplay.GetMetrics(displayMetrics);
        return displayMetrics;
    }

    // http://stackoverflow.com/a/10127787/72174
    private static int CalculateInSampleSize(BitmapFactory.Options actualImageParams, int maxAllowedWidth, int maxAllowedHeight)
    {
        // Raw height and width of image
        int height = actualImageParams.OutHeight;
        int width = actualImageParams.OutWidth;
        int inSampleSize = 1;

        if (height > maxAllowedHeight || width > maxAllowedWidth)
        {

            int halfHeight = height / 2;
            int halfWidth = width / 2;

            // Calculate the largest inSampleSize value that is a power of 2 and keeps both
            // height and width larger than the requested height and width.
            while (halfHeight / inSampleSize > maxAllowedHeight || halfWidth / inSampleSize > maxAllowedWidth)
            {
                inSampleSize *= 2;
            }
        }

        return inSampleSize;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        var i = Target;
        if (i != null)
        {
            if (i.Drawable is BitmapDrawable d)
            {
                Bitmap bitmap = d.Bitmap;
                if (bitmap != null)
                {
                    bitmap.Recycle();
                    bitmap.Dispose();
                }
            }

            i.SetImageBitmap(null);
            i.SetImageDrawable(null);
        }
    }
}
