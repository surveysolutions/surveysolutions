using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Android.Widget;
using Cirrious.MvvmCross.Binding;


namespace WB.UI.Tester.CustomBindings
{
    public class TextViewHintBinding : BaseBinding<TextView, string>
    {
        private bool subscribed;

        public TextViewHintBinding(TextView target)
            : base(target)
        {
            Target.TextChanged += TextChangedHandler;
            subscribed = true;
        }

        protected override void SetValueToView(TextView view, string value)
        {
            view.Hint = value;
            SetupHintStyle(view.Text);
        }


        private void TextChangedHandler(object sender, TextChangedEventArgs e)
        {
            if (e.AfterCount > 0 && e.BeforeCount > 0 && Target.Typeface.IsBold)
                return;

            if (e.AfterCount == 0 && e.BeforeCount == 0 && Target.Typeface.IsItalic)
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

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var editText = Target;
                if (editText != null && subscribed)
                {
                    editText.TextChanged -= TextChangedHandler;
                    subscribed = false;
                }
            }
            base.Dispose(isDisposing);
        }
    }
}
