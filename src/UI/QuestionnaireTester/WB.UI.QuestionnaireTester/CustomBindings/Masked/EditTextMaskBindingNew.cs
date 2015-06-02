using System.Linq;
using Android.Content.Res;
using Android.Provider;
using Android.Runtime;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.MaskFormatter;


namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextMaskBindingNew : BaseBinding<EditText, string>
    {
        public EditTextMaskBindingNew(EditText target)
            : base(target)
        {
        }

        protected override void SetValueToView(EditText view, string value)
        {
            bool isInputMasked = !string.IsNullOrWhiteSpace(value);

            if (isInputMasked)
            {
                //maskedWatcher = new MaskedWatcher(value, Target);
                //Target.AddTextChangedListener(maskedWatcher);
                //Target.InputType = InputTypes.TextVariationVisiblePassword; //fix for samsung 
            }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = Target;
                if (editText != null)
                {
                    //editText.RemoveTextChangedListener(maskedWatcher);
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
