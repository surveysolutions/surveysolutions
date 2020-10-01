using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class InstantAutoCompleteTextViewDisableDefaultSearchBinding : BaseBinding<InstantAutoCompleteTextView, bool>
    {
        public InstantAutoCompleteTextViewDisableDefaultSearchBinding(InstantAutoCompleteTextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(InstantAutoCompleteTextView control, bool value)
        {
            if (value)
            {
                control.DisableDefaultSearch();
            }
        }
    }
}
