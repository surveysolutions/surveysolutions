using Android.Widget;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.Utils;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewHtmlBinding : BaseBinding<TextView, string>
    {
        public TextViewHtmlBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(TextView control, string value)
        {
            control.SetText(value.ToAndroidSpanned(), TextView.BufferType.Spannable);
        }
    }
}