using Android.Widget;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class TextViewHintBinding : BindingWrapper<TextView, string>
    {
        public TextViewHintBinding(TextView target)
            : base(target)
        {
        }

        protected override void SetValueToView(TextView view, string value)
        {
            Target.Hint = value;
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}
