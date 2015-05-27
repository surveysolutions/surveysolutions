using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class FocusTextForEditTextBinding : BindingWrapper<EditText, string>
    {
        public FocusTextForEditTextBinding(EditText view)
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
            if (this.EditText == null)
                return;

            this.FireValueChanged(this.EditText.Text);
        }
    }
}