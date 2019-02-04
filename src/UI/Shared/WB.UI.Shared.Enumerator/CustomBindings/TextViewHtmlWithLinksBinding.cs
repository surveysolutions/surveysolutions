using Android.Text.Method;
using Android.Widget;
using Java.Lang;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewHtmlWithLinksBinding : BaseBinding<TextView, ICharSequence>
    {
        public TextViewHtmlWithLinksBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, ICharSequence value)
        {
            control.SetText(value, TextView.BufferType.Spannable);
            control.MovementMethod = LinkMovementMethod.Instance;
        }
    }
}
