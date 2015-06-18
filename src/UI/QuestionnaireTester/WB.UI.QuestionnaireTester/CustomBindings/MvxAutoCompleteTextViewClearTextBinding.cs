using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class MvxAutoCompleteTextViewClearTextBinding : BaseBinding<MvxAutoCompleteTextView, bool>
    {
        public MvxAutoCompleteTextViewClearTextBinding(MvxAutoCompleteTextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAutoCompleteTextView control, bool value)
        {
            if (value)
            {
                Target.ClearListSelection();
                Target.DismissDropDown();
                Target.Text = string.Empty;
            }
        }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.ItemClick+= this.OnItemClick;
            }

            base.SubscribeToEvents();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.ItemClick -= this.OnItemClick;
            }

            base.Dispose(isDisposing);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            Target.ClearFocus();
        }
    }
}