using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class FocusTextForEditTextBinding : BindingWrapper<EditText, string>
    {
        private bool subscribed;

        public FocusTextForEditTextBinding(EditText view)
            : base(view)
        {
            view.ImeOptions = ImeAction.Done;
        }

        protected override void SetValueToView(EditText androidControl, string value)
        {
            var editText = androidControl;
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
            var editText = Target;
            if (editText == null)
                return;

            editText.FocusChange += HandleFocusChange;
            editText.EditorAction += HandleEditorAction;
            subscribed = true;
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
                var editText = Target;
                if (editText != null && subscribed)
                {
                    editText.FocusChange -= HandleFocusChange;
                    editText.EditorAction -= HandleEditorAction;
                    subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }

        private void TriggerValueChanging()
        {
            var editText = this.Target;
            if (editText == null)
            {
                return;
            }

            this.FireValueChanged(editText.Text);
        }
    }
}