using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidApp.Bindings
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