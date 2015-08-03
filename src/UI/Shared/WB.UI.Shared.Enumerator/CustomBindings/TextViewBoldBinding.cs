using Android.Graphics;
using Android.Widget;

namespace WB.UI.Tester.CustomBindings
{
    public class TextViewBoldBinding : BaseBinding<TextView, bool>
    {
        public TextViewBoldBinding(TextView textView)
            : base(textView) {}

        protected override void SetValueToView(TextView textView, bool bold)
        {
            textView.SetTypeface(Typeface.Default, bold ? TypefaceStyle.Bold : TypefaceStyle.Normal);
        }
    }
}