using System;
using Android.Graphics;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewBoldBinding : MvxAndroidTargetBinding
    {
        public TextViewBoldBinding(TextView textView)
            : base(textView) {}

        public override Type TargetType
        {
            get { return typeof(bool); }
        }

        protected override void SetValueImpl(object target, object value)
        {
            var bold = (bool) value;
            var textView = target as TextView;

            if (textView == null) return;

            textView.SetTypeface(Typeface.Default, bold ? TypefaceStyle.Bold : TypefaceStyle.Normal);
        }
    }
}