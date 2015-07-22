using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.Tester.CustomBindings
{
    public class MvxAutoCompleteTextViewResetTextBinding : BaseBinding<MvxAutoCompleteTextView, string>
    {
        public MvxAutoCompleteTextViewResetTextBinding(MvxAutoCompleteTextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }

        protected override void SetValueToView(MvxAutoCompleteTextView control, string value)
        {
            Target.ClearListSelection();
            Target.DismissDropDown();

            // this is hack. http://www.grokkingandroid.com/how-androids-autocompletetextview-nearly-drove-me-nuts/
            var adapter = Target.Adapter;
            Target.Adapter = null;

            if (value == null)
            {
                Target.Text = string.Empty;
            }
            else
            {
                Target.SetText(value, true);
                Target.SetSelection(value.Length);
            }

            Target.Adapter = adapter;
        }

        public override void SubscribeToEvents()
        {
            if (this.Target != null)
            {
                this.Target.ItemClick += this.OnItemClick;
                this.Target.FocusChange += TargetOnFocusChange;
                this.Target.EditorAction += HandleEditorAction;
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
                this.Target.FocusChange -= TargetOnFocusChange;
                this.Target.EditorAction -= HandleEditorAction;

            }

            base.Dispose(isDisposing);
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            Target.ClearFocus();
        }

        private void HandleEditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            Target.DismissDropDown();
        }
    }
}