using Android.Widget;
using WB.UI.QuestionnaireTester.CustomBindings.Masked;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextMaskedTextBinding : EditTextFocusTextBinding
    {
        private readonly EditTextMaskWrapper editTextMaskWrapper;

        public EditTextMaskedTextBinding(MaskedEditText target)
            : base(target)
        {
            this.editTextMaskWrapper = new EditTextMaskWrapper(target);
        }

        protected override bool IsAllowFireEvent()
        {
            return editTextMaskWrapper.IsAnswered;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.editTextMaskWrapper.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}
