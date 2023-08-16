using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using WB.UI.Shared.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.CustomBindings;

public class ImageViewBitmapWithFallbackBinding : BaseBinding<ImageView, byte[]>
{
    private const int MaxAllowedDimension = 400; 
    
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
            BitmapFactory.Options boundsOptions = new BitmapFactory.Options { InPurgeable = true };
            using (var initBitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions)) // To determine actual image size
            {
                //if source image is smaller limit - do not transform it 
                if (boundsOptions.OutHeight <= MaxAllowedDimension && boundsOptions.OutWidth <= MaxAllowedDimension)
                {
                    SetupImageView(control, displayMetrics, boundsOptions);
                    control.Drawable?.Dispose();
                    control.SetImageBitmap(initBitmap);
                    return;
                }

                var bitmap = BitmapHelper.GetBitmapOrNull(value, boundsOptions, minSize);
                if (bitmap != null)
                {
                    SetupImageView(control, displayMetrics, boundsOptions);
                    control.Drawable?.Dispose();
                    control.SetImageBitmap(bitmap);
                    
                    bitmap.Dispose();
                }
                else
                {
                    control.Drawable?.Dispose();
                    this.SetDefaultImage(control);
                }
            }
        }
        else
        {
            control.Drawable?.Dispose();
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

    //temp excluding 
    //to check crashes
    /*protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        var i = Target;
        if (i == null) return;
        
        if (i.Drawable is BitmapDrawable d)
        {
            var bitmap = d.Bitmap;
            if (bitmap != null && !bitmap.IsRecycled)
            {
                bitmap.Recycle();
                bitmap.Dispose();
            }
        }

        i.SetImageBitmap(null);
        i.SetImageDrawable(null);
    }*/
}
