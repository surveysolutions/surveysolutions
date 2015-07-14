using System;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.Tester.CustomBindings
{
    public class TextViewHtmlBinding : BaseBinding<TextView, string>
    {
        public TextViewHtmlBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode { get { return MvxBindingMode.OneWay; } }

        protected override void SetValueToView(TextView control, string value)
        {
            control.TextFormatted = Html.FromHtml(value ?? String.Empty);
        }
    }
}