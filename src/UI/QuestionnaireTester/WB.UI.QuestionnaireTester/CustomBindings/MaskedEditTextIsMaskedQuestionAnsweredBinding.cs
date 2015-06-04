using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;
using WB.UI.QuestionnaireTester.CustomBindings.Masked;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class MaskedEditTextIsMaskedQuestionAnsweredBinding : BaseBinding<MaskedEditText, bool>
    {
        public MaskedEditTextIsMaskedQuestionAnsweredBinding(MaskedEditText target)
            : base(target)
        {
        }

        public override void SubscribeToEvents()
        {
            Target.IsMaskedFormAnsweredChanged += this.IsMaskedFormAnsweredChangedHandler;
        }

        protected override void SetValueToView(MaskedEditText control, bool value)
        {
            // ignore, we only read
        }

        private void IsMaskedFormAnsweredChangedHandler(object sender, bool isAnswered)
        {
            var target = this.Target;

            if (target == null)
                return;

            var value = target.IsMaskedFormAnswered;
            FireValueChanged(value);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var target = Target as MaskedEditText;
                if (target != null)
                {
                    target.IsMaskedFormAnsweredChanged -= this.IsMaskedFormAnsweredChangedHandler;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}