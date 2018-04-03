using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Text;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewWatermarkBinding : BaseBinding<TextView, string>
    {
        private bool subscribed;

        public TextViewWatermarkBinding(TextView target)
            : base(target)
        {
            this.Target.TextChanged += this.TextChangedHandler;
            this.subscribed = true;
        }

        protected override void SetValueToView(TextView view, string value)
        {
            view.Hint = value;
            this.SetupHintStyle(view.Text);
        }


        private void TextChangedHandler(object sender, TextChangedEventArgs e)
        {
            if (e.AfterCount > 0 && e.BeforeCount > 0 && this.Target.Typeface.IsBold)
                return;

            if (e.AfterCount == 0 && e.BeforeCount == 0 && this.Target.Typeface.IsItalic)
                return;
           
            this.SetupHintStyle(e.Text);
        }

        private void SetupHintStyle(IEnumerable<char> text)
        {
            if (text.Any())
            {
                this.Target.SetTypeface(null, TypefaceStyle.Bold);
            }
            else
            {
                this.Target.SetTypeface(null, TypefaceStyle.Italic);
            }
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            if (isDisposing)
            {
                var editText = this.Target;
                if (editText != null && this.subscribed)
                {
                    editText.TextChanged -= this.TextChangedHandler;
                    this.subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
