using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class EditTextFocusChangeTextBinding : BindingWrapper<EditText, string>
    {
        private bool subscribed;

        public EditTextFocusChangeTextBinding(EditText view)
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
            subscribed = true;
        }

        private void HandleFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            var editText = Target;
            if (editText == null)
                return;

            if (!e.HasFocus)
                FireValueChanged(editText.Text);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = Target;
                if (editText != null && subscribed)
                {
                    editText.FocusChange -= HandleFocusChange;
                    subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}