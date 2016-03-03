using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Support.V7.AppCompat.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MvxAutoCompleteTextViewHidePopupOnDoneBinding : BaseBinding<MvxAppCompatAutoCompleteTextView, object>
    {
        public MvxAutoCompleteTextViewHidePopupOnDoneBinding(MvxAppCompatAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAppCompatAutoCompleteTextView view, object value) { }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.EditorAction += this.HandleEditorAction;
            }

            base.SubscribeToEvents();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.EditorAction -= this.HandleEditorAction;
            }

            base.Dispose(isDisposing);
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            this.Target.DismissDropDown();
        }
    }
}