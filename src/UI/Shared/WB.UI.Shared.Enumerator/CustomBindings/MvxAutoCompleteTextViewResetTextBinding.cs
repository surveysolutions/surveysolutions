using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.Binding.Droid.Views;
using MvvmCross.Droid.Support.V7.AppCompat.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MvxAutoCompleteTextViewResetTextBinding : BaseBinding<MvxAppCompatAutoCompleteTextView, string>
    {
        public MvxAutoCompleteTextViewResetTextBinding(MvxAppCompatAutoCompleteTextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAppCompatAutoCompleteTextView control, string value)
        {
            this.Target.ClearListSelection();
            this.Target.DismissDropDown();

            // this is hack. http://www.grokkingandroid.com/how-androids-autocompletetextview-nearly-drove-me-nuts/
            var adapter = this.Target.Adapter;
            this.Target.Adapter = null;

            if (value == null)
            {
                this.Target.Text = string.Empty;
            }
            else
            {
                this.Target.SetText(value, true);
                this.Target.SetSelection(value.Length);
            }

            this.Target.Adapter = adapter;
        }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.ItemClick += this.OnItemClick;
                this.Target.FocusChange += this.TargetOnFocusChange;
                this.Target.EditorAction += this.HandleEditorAction;
            }

            base.SubscribeToEvents();
        }

        private void TargetOnFocusChange(object sender, View.FocusChangeEventArgs focusChangeEventArgs)
        {
            if (focusChangeEventArgs.HasFocus)
            {
                if (string.IsNullOrEmpty(this.Target.Text))
                {
                    this.Target.ShowDropDown();
                }
            }
            else
            {
                this.Target.DismissDropDown();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && this.Target != null)
            {
                this.Target.ItemClick -= this.OnItemClick;
                this.Target.FocusChange -= this.TargetOnFocusChange;
                this.Target.EditorAction -= this.HandleEditorAction;

            }

            base.Dispose(isDisposing);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            this.Target.ClearFocus();
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            this.Target.DismissDropDown();
        }
    }
}