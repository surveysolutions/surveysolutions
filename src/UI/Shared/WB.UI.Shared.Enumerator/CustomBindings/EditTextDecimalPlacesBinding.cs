using Android.Text;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextDecimalPlacesBinding : BaseBinding<EditText, int?>
    {
        public EditTextDecimalPlacesBinding(EditText androidControl)
            : base(androidControl)
        {
        }

        private EditText EditText
        {
            get { return this.Target; }
        }

        protected override void SetValueToView(EditText control, int? value)
        {
            if (value.HasValue)
                this.EditText.SetFilters(new IInputFilter[] { new DecimalPlacesFilter(value.Value) });
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneTime; }
        }
    }
}