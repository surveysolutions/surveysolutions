using System;
using Android.Text;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewHtmlBinding : BaseBinding<TextView, string>
    {
        public TextViewHtmlBinding(TextView androidControl) : base(androidControl) { }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void SetValueToView(TextView control, string value)
        {
            control.SetText(Html.FromHtml(value ?? String.Empty), TextView.BufferType.Spannable);
            //don't use
            //control.TextFormatted = Html.FromHtml(value ?? String.Empty);
            //height is wrong calculated
        }
    }
}