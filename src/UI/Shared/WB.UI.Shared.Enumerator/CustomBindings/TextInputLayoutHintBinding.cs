#nullable enable
using System.Windows.Input;
using Android.Views;
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
    
    public class TextInputLayoutEndIconClickBinding : BaseBinding<TextInputLayout, ICommand>
    {
        public TextInputLayoutEndIconClickBinding(TextInputLayout androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextInputLayout control, ICommand value)
        {
            control.SetEndIconOnClickListener(new IconClickListener(value));
        }
    }
    
    public class IconClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private readonly ICommand value;

        public IconClickListener(ICommand value)
        {
            this.value = value;
        }

        public void OnClick(View? v)
        {
            if (v != null)
            {
                value.Execute(null);
            }
        }
    }
}
