using System.IO;
using Android.Graphics;
using FFImageLoading;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.UI.Shared.Enumerator.Services;

public class BitmapHelper : IImageHelper
{
    public static Bitmap GetTransformedBitmapOrNull(byte[] value, int maxAllowedDimension)
    {
        // Calculate inSampleSize
        BitmapFactory.Options boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true, InPurgeable = true };
        using (BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions)) // To determine actual image size
        {
            int sampleSize = CalculateInSampleSize(boundsOptions, maxAllowedDimension, maxAllowedDimension);
            var bitmapOptions = new BitmapFactory.Options { InSampleSize = sampleSize, InPurgeable = true };
            using (var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions))
            {
                return bitmap;
            }
        }
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
    
    public byte[] GetTransformedArrayOrNull(byte[] value, int maxAllowedDimension)
    {
        // Calculate inSampleSize
        BitmapFactory.Options boundsOptions = new BitmapFactory.Options { InJustDecodeBounds = true, InPurgeable = true };
        using (BitmapFactory.DecodeByteArray(value, 0, value.Length, boundsOptions)) // To determine actual image size
        {
            if (boundsOptions.OutHeight < maxAllowedDimension && boundsOptions.OutWidth < maxAllowedDimension)
            {
                return null;
            }

            int sampleSize = CalculateInSampleSize(boundsOptions, maxAllowedDimension, maxAllowedDimension);
            var bitmapOptions = new BitmapFactory.Options { InSampleSize = sampleSize, InPurgeable = true };
            using (var bitmap = BitmapFactory.DecodeByteArray(value, 0, value.Length, bitmapOptions))
            {
                if (bitmap == null)
                    return null;
                MemoryStream outputStream = new MemoryStream();
                if (bitmap.Compress(Bitmap.CompressFormat.Png, 100, outputStream))
                {
                    bitmap.Recycle();
                    return outputStream.ToByteArray();
                }
                
                bitmap.Recycle();
                return null;
            }
        }
    }
}
