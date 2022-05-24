using System;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Text;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewWatermarkBinding : BaseBinding<TextView, string>
    {
        private IDisposable subscription;
        public TextViewWatermarkBinding(TextView target)
            : base(target)
        {
        }
        
        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (this.Target == null)
                return;

            subscription = this.Target.WeakSubscribe<TextView, TextChangedEventArgs>(
                nameof(this.Target.TextChanged),
                TextChangedHandler);
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
            if (isDisposing)
            {
                subscription?.Dispose();
                subscription = null;
            }

            base.Dispose(isDisposing);
        }
    }
}
