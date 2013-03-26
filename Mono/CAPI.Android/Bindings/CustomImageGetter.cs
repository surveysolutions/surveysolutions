using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;

namespace CAPI.Android.Bindings
{
    public class CustomImageGetter : Html.IImageGetter
    {
        #region Implementation of IDisposable

        public void Dispose()
        {
          //  throw new NotImplementedException();
        }

        #endregion

        #region Implementation of IJavaObject

        public IntPtr Handle { get; private set; }
        public Drawable GetDrawable(string source)
        {
            byte[] data = Base64.Decode(source, Base64Flags.Default);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);

            return new BitmapDrawable(CapiApplication.Context.Resources, bitmap);
        }

        #endregion
    }
}