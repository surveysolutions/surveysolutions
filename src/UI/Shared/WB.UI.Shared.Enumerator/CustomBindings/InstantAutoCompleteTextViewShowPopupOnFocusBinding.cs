using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewShowPopupOnFocusBinding : BaseBinding<InstantAutoCompleteTextView, object>
    {
        public InstantAutoCompleteTextViewShowPopupOnFocusBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(InstantAutoCompleteTextView view, object value) { }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWayToSource;

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.ItemClick += this.OnItemClick;
                this.Target.FocusChange += this.TargetOnFocusChange;
                this.Target.Click += Target_Click;
            }

            base.SubscribeToEvents();
        }

        private void Target_Click(object sender, System.EventArgs e)
        {
            if(!this.Target.IsPopupShowing && this.Target.HasFocus)
                this.Target.ShowDropDown();
        }

        private void TargetOnFocusChange(object sender, View.FocusChangeEventArgs focusChangeEventArgs)
        {
            if (focusChangeEventArgs.HasFocus && !this.Target.IsPopupShowing)
                this.Target.ShowDropDown();

            if (!focusChangeEventArgs.HasFocus && this.Target.IsPopupShowing)
                this.Target.DismissDropDown();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.ItemClick -= this.OnItemClick;
                this.Target.FocusChange -= this.TargetOnFocusChange;
                this.Target.Click -= this.Target_Click;
            }

            base.Dispose(isDisposing);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            this.Target.ClearFocus();
        }
    }
}