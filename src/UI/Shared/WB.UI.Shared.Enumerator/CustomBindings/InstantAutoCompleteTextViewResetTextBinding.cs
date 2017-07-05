using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewResetTextBinding : BaseBinding<InstantAutoCompleteTextView, string>
    {
        public InstantAutoCompleteTextViewResetTextBinding(InstantAutoCompleteTextView androidControl)
            : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;

        protected override void SetValueToView(InstantAutoCompleteTextView control, string value)
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

        
    }
}