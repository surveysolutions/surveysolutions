using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class NumericDisableBinding : BaseBinding<NumericEditText, bool>
    {
        public NumericDisableBinding(NumericEditText textView)
            : base(textView) {}

        protected override void SetValueToView(NumericEditText textView, bool isDisabled)
        {
            textView.Enabled = !isDisabled;
        }
    }
}