using Android.Views;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MvxAutoCompleteTextViewShowPopupOnFocusBinding : BaseBinding<MvxAutoCompleteTextView, object>
    {
        public MvxAutoCompleteTextViewShowPopupOnFocusBinding(MvxAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(MvxAutoCompleteTextView view, object value) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.TwoWay;} }

        public override void SubscribeToEvents()
        {
            this.Target.FocusChange += this.Target_FocusChange;

            base.SubscribeToEvents();
        }

        private void Target_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                this.Target.ShowDropDown();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = this.Target;
                if (editText != null)
                {
                    editText.FocusChange -= this.Target_FocusChange;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}