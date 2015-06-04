using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextFocusTextBinding : BaseBinding<EditText, string>
    {
        public EditTextFocusTextBinding(EditText view)
            : base(view)
        {
            view.ImeOptions = ImeAction.Done;
        }

        private EditText EditText
        {
            get { return this.Target; }
        }

        protected override void SetValueToView(EditText editText, string value)
        {
            if (editText == null)
                return;

            editText.Text = value;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
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

            TriggerValueChanging();

            HideKeyboard();
        }

        private void HandleFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus) return;

            TriggerValueChanging();
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

        private void TriggerValueChanging()
        {
            if (this.EditText == null || !IsAllowFireEvent())
                return;

            this.FireValueChanged(this.EditText.Text);
        }

        protected virtual bool IsAllowFireEvent()
        {
            return true;
        }


        private void HideKeyboard()
        {
            var activity = (Activity)EditText.Context;
            var windowToken = EditText.WindowToken;

            var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(windowToken, 0);
        }
    }
}