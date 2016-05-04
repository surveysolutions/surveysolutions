using Android.Text;
using Android.Text.Style;
using Android.Widget;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class TextViewIsCurrentDashboardTabBinding : BaseBinding<TextView, bool>
    {
        public TextViewIsCurrentDashboardTabBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.OneWay; } }

        protected override void SetValueToView(TextView control, bool isNeedUnderlineText)
        {
            if (isNeedUnderlineText)
            {
                var value = control.Text;
                SpannableString content = new SpannableString(value);
                content.SetSpan(new UnderlineSpan(), 0, value.Length, 0);
                control.TextFormatted = content;
            }
            else
            {
                control.Text = control.Text;
            }
        }
    }
}