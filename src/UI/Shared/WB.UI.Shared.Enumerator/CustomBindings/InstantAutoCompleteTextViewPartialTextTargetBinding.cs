using System;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewPartialTextTargetBinding : BaseBinding<InstantAutoCompleteTextView, string>
    {
        public InstantAutoCompleteTextViewPartialTextTargetBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(InstantAutoCompleteTextView control, string value)
        {
            if (!string.Equals(value ?? String.Empty, control.Text))
            {
                control.SetText(value);
            }
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
    }
}
