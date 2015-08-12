using System;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.Shared.Android.Bindings
{
    public class HtmlBinding : MvvmBindingWrapper<TextView>
    {
        private Html.IImageGetter imageGetter;
        public HtmlBinding(TextView control):base(control)
        {
            this.imageGetter = new ImageGetter(control.Resources);
            control.MovementMethod = LinkMovementMethod.Instance;
        }

        protected override void SetValueToView(TextView view, object value)
        {
            var htmlString = (string)value;
            view.SetText(Html.FromHtml(htmlString, this.imageGetter, null), TextView.BufferType.Spannable);
            
        }

        public override Type TargetType
        {
            get { return typeof(string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.imageGetter != null)
                {
                    this.imageGetter.Dispose();
                    this.imageGetter = null;
                }
            }

            base.Dispose(isDisposing);
        }
    }

    public class ImageGetter : Java.Lang.Object, Html.IImageGetter
    {
        private readonly global::Android.Content.Res.Resources resources;

        public ImageGetter(global::Android.Content.Res.Resources resources)
        {
            this.resources = resources;
        }

        public Drawable GetDrawable(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;

            var imageString = source.Split(',').LastOrDefault();
            if (string.IsNullOrEmpty(imageString))
                return null;

            byte[] data = Base64.Decode(imageString, 0);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            
            if (bitmap == null)
                return null;

            var result = new BitmapDrawable(this.resources, bitmap);

            result.Bounds = new Rect(0, 0, bitmap.Width, bitmap.Height);
            return result;
        }
    }
}