﻿using Android.Content;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

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


        public override void SubscribeToEvents()
        {
            base.SubscribeToEvents();

            if (this.Target == null)
                return;

            this.Target.Touch += Target_Touch;
        }

        private void Target_Touch(object sender, Android.Views.View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                var decoratedContent = new SpannableString(this.Target.Text);
                this.Target.TextFormatted = decoratedContent;
            }
            else if (e.Event.Action == MotionEventActions.Up)
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
                if (this.Target != null)
                {
                    this.Target.Touch -= Target_Touch;
                }
            }
            base.Dispose(isDisposing);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }
    }
}