using Android.Views;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.Tester.CustomBindings
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
            Target.FocusChange += Target_FocusChange;

            base.SubscribeToEvents();
        }

        private void Target_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Target.ShowDropDown();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = Target;
                if (editText != null)
                {
                    editText.FocusChange -= Target_FocusChange;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}