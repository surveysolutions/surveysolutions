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
        private View.IOnClickListener? onClickListener;
        
        public TextInputLayoutEndIconClickBinding(TextInputLayout androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextInputLayout control, ICommand value)
        {
            onClickListener = new IconClickListener(value);
            control.SetEndIconOnClickListener(onClickListener);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (isDisposing)
            {
                onClickListener?.Dispose();
                onClickListener = null;
            }
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
