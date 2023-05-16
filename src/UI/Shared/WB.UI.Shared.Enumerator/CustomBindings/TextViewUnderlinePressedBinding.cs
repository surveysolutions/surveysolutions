using System;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding;
using MvvmCross.WeakSubscription;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewUnderlinePressedBinding : BaseBinding<TextView, bool>
    {
        public TextViewUnderlinePressedBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, bool value)
        {
            var decoratedContent = new SpannableString(this.Target.Text);
            decoratedContent.SetSpan(new UnderlineSpan(), 0, decoratedContent.Length(), 0);
            this.Target.TextFormatted = decoratedContent;
        }

        private IDisposable subscription;

        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            var textView = Target;
            if (textView == null)
                return;

            subscription = textView.WeakSubscribe<TextView, TextView.TouchEventArgs>(
                nameof(textView.Touch),
                OnTouch);
        }

        private void OnTouch(object sender, TextView.TouchEventArgs e)
        {
            if (e.Event?.Action == MotionEventActions.Down)
            {
                var decoratedContent = new SpannableString(this.Target.Text);
                this.Target.TextFormatted = decoratedContent;
            }
            else if (e.Event?.Action == MotionEventActions.Up)
            {
                var decoratedContent = new SpannableString(this.Target.Text);
                decoratedContent.SetSpan(new UnderlineSpan(), 0, decoratedContent.Length(), 0);
                this.Target.TextFormatted = decoratedContent;
            }

            e.Handled = false;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                subscription?.Dispose();
                subscription = null;
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.TwoWay;
    }
}
