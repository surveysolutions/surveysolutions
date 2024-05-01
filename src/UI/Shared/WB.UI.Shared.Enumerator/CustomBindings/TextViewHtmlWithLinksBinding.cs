using Android.Text;
using Android.Text.Method;
using Android.Widget;
using Java.Lang;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public partial class TextViewHtmlWithLinksBinding : BaseBinding<TextView, object>
    {
        public TextViewHtmlWithLinksBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, object value)
        {
            if (value is ICharSequence charSequence)
            {
                control.SetText(charSequence, TextView.BufferType.Spannable);
                
                if (value is SpannableStringBuilder)
                    control.MovementMethod = LinkMovementMethod.Instance;
            }
            else if (value is string stringValue)
            {
                control.SetText(stringValue.ToAndroidSpanned(), TextView.BufferType.Spannable);
            }
        }
    }
}
