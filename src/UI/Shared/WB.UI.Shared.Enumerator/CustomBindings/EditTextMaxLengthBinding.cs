using Android.Text;
using Android.Widget;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class EditTextMaxLengthBinding : BaseBinding<EditText, int>
    {
        public EditTextMaxLengthBinding(EditText androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(EditText control, int value) 
            => control.SetFilters(new IInputFilter[] {new InputFilterLengthFilter(value)});
    }
}
