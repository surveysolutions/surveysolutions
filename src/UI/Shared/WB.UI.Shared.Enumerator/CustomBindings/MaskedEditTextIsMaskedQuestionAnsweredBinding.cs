using System;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;
using WB.UI.Shared.Enumerator.CustomControls.MaskedEditTextControl;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MaskedEditTextIsMaskedQuestionAnsweredBinding : BaseBinding<MaskedEditText, bool>
    {
        public MaskedEditTextIsMaskedQuestionAnsweredBinding(MaskedEditText target) : base(target) {}

        private IDisposable subscription;
        public override void SubscribeToEvents()
        {
            var target = Target;
            if (target == null)
                return;
            
            subscription = target.WeakSubscribe<MaskedEditText, EventArgs>(
                nameof(target.IsMaskedFormAnsweredChanged),
                (sender, args) => {
                    var value = this.Target?.IsMaskedFormAnswered;
                    this.FireValueChanged(value); }
            );
        }

        protected override void SetValueToView(MaskedEditText control, bool value)
        {
            // ignore, we only read this property from control
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                subscription?.Dispose();
                subscription = null;
            }
            
            base.Dispose(isDisposing);
        }
    }
}
