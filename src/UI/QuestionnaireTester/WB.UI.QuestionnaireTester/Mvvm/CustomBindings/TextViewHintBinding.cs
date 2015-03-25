using System;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.Mvvm.CustomBindings
{
    public class TextViewHintBinding : MvvmBindingWrapper<TextView>
    {
        public TextViewHintBinding(TextView target)
            : base(target)
        {
        }

        protected override void SetValueToView(TextView view, object value)
        {
            Target.Hint = (string)value;
        }

        public override Type TargetType
        {
            get { return typeof (string); }
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}
