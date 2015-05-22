using Android.Content;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class SetFocusForEditTextBinding : BindingWrapper<EditText, bool>
    {
        private bool subscribed;

        public SetFocusForEditTextBinding(EditText editText)
            : base(editText)
        {
            editText.ShowSoftInputOnFocus = true;
        }

        protected override void SetValueToView(EditText androidControl, bool value)
        {
            var editText = androidControl;
            if (editText == null)
                return;

            if (value)
            {
                editText.RequestFocus();
                InputMethodManager imm = (InputMethodManager)editText.Context.GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(editText, ShowFlags.Implicit);
            }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}