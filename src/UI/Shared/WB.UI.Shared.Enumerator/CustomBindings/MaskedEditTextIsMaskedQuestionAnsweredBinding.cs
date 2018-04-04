using System;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls.MaskedEditTextControl;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MaskedEditTextIsMaskedQuestionAnsweredBinding : BaseBinding<MaskedEditText, bool>
    {
        public MaskedEditTextIsMaskedQuestionAnsweredBinding(MaskedEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            this.Target.IsMaskedFormAnsweredChanged += this.IsMaskedFormAnsweredChangedHandler;
        }

        protected override void SetValueToView(MaskedEditText control, bool value)
        {
            // ignore, we only read this property from control
        }

        private void IsMaskedFormAnsweredChangedHandler(object sender, EventArgs e)
        {
            var target = this.Target;

            if (target == null)
                return;

            var value = target.IsMaskedFormAnswered;
            this.FireValueChanged(value);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                var target = this.Target as MaskedEditText;
                if (target != null)
                {
                    target.IsMaskedFormAnsweredChanged -= this.IsMaskedFormAnsweredChangedHandler;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
