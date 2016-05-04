using Android.Text;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewTextFormattedBinding : BaseBinding<TextView, ISpannable>
    {
        public TextViewTextFormattedBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.OneWay; } }

        protected override void SetValueToView(TextView control, ISpannable value)
        {
            control.TextFormatted = value;
        }
    }
}