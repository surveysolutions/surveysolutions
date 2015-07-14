using System;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewTextFormattedBinding : BaseBinding<TextView, ISpannable>
    {
        public TextViewTextFormattedBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.OneWay; } }

        protected override void SetValueToView(TextView control, ISpannable value)
        {
            control.TextFormatted = value;
        }
    }
}