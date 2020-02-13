using Android.Text;
using Android.Text.Method;
using Android.Widget;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewHyperLinkBinding : BaseBinding<TextView, NavigationModel>
    {
        public TextViewHyperLinkBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, NavigationModel value)
        {
            control.SetText(
                Html.FromHtml($"<a href=\"{value?.Url}\">{value?.Text}</a>", FromHtmlOptions.ModeCompact),
                TextView.BufferType.Spannable);

            control.MovementMethod = LinkMovementMethod.Instance;
        }
    }
}
