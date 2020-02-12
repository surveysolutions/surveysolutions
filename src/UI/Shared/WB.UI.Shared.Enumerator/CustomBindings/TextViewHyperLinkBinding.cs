using Android.Text;
using Android.Text.Method;
using Android.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewHyperLinkBinding : BaseBinding<TextView, (string text, string url)?>
    {
        public TextViewHyperLinkBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, (string text, string url)? value)
        {
            control.SetText(
                Html.FromHtml(value == null ? "" : $"<a href=\"{value.Value.url}\">{value.Value.text}</a>", FromHtmlOptions.ModeCompact),
                TextView.BufferType.Spannable);

            control.MovementMethod = LinkMovementMethod.Instance;
        }
    }
}
