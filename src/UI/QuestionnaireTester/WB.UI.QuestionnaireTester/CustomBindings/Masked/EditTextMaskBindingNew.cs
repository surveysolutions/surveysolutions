using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.UI.QuestionnaireTester.CustomBindings.Masked;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextMaskBindingNew : BaseBinding<EditText, string>
    {
        private readonly EditTextMaskWrapper editTextMaskWrapper;

        public EditTextMaskBindingNew(EditText target)
            : base(target)
        {
            this.editTextMaskWrapper = new EditTextMaskWrapper(target);
        }

        protected override void SetValueToView(EditText view, string value)
        {
            this.editTextMaskWrapper.Mask = value;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
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
