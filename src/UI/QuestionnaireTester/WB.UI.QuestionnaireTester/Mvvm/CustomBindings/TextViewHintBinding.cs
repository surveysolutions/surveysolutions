using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Droid.Target;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{
    public class TextViewHintBinding : MvxAndroidTargetBinding
    {
        protected new TextView Target
        {
            get { return (TextView)base.Target; }
        }

        public TextViewHintBinding(TextView target)
            : base(target)
        {
        }

        protected override void SetValueImpl(object target, object value)
        {
            if (Target == null)
                return;

            Target.Hint = (string)value;
        }

        public override Type TargetType
        {
            get { return typeof (string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.TwoWay; }
        }
    }
}
