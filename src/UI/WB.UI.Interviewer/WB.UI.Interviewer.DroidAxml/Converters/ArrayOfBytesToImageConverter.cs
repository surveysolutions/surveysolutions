using System;
using System.Globalization;
using System.IO;
using Android.Graphics;
using Cirrious.CrossCore.Converters;

namespace AxmlTester.Droid.Converters
{
    public class ByteArrayToImageConverter :
      MvxValueConverter<byte[], Bitmap>
    {
        protected override Bitmap Convert(byte[] value,
          Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var options = new BitmapFactory.Options { InPurgeable = true };
            return BitmapFactory.DecodeByteArray(
              value, 0, value.Length, options);
        }
    }
}
