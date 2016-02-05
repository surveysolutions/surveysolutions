using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Views;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MvxAutoCompleteTextViewHidePopupOnDoneBinding : BaseBinding<MvxAutoCompleteTextView, object>
    {
        public MvxAutoCompleteTextViewHidePopupOnDoneBinding(MvxAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAutoCompleteTextView view, object value) { }

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