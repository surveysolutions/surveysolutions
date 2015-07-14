using Android.Content;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextSetFocusBinding : BaseBinding<EditText, bool>
    {
        public EditTextSetFocusBinding(EditText editText)
            : base(editText)
        {
            editText.ShowSoftInputOnFocus = true;
        }

        private EditText EditText
        {
            get { return this.Target; }
        }

        protected override void SetValueToView(EditText control, bool value)
        {
            var editText = control;
            if (editText == null)
                return;

            if (value)
            {
                editText.RequestFocus();
                InputMethodManager imm = (InputMethodManager)editText.Context.GetSystemService(Context.InputMethodService);
                imm.ShowSoftInput(editText, ShowFlags.Implicit);
            }
        }

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (this.EditText == null)
                return;

            this.EditText.FocusChange += this.HandleFocusChange;
            this.EditText.EditorAction += this.HandleEditorAction;
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            e.Handled = false;

            if (e.ActionId != ImeAction.Done) return;

            e.Handled = true;

            this.TriggerValueChangeToFalse();
        }

        private void HandleFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus) return;

            this.TriggerValueChangeToFalse();
        }

        private void TriggerValueChangeToFalse()
        {
            if (this.EditText == null)
                return;

            this.FireValueChanged(false);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (this.EditText != null)
                {
                    this.EditText.FocusChange -= this.HandleFocusChange;
                    this.EditText.EditorAction -= this.HandleEditorAction;
                }
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }
    }
}