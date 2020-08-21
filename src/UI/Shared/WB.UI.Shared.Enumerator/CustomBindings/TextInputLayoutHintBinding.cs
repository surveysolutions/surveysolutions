using Com.Google.Android.Exoplayer2.Util;
using Google.Android.Material.TextField;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextInputLayoutHintBinding : BaseBinding<TextInputLayout, string>
    {
        public TextInputLayoutHintBinding(TextInputLayout androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextInputLayout control, string value)
        {
            control.Hint = value;
        }
    }
}
