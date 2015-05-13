using Android.Graphics;
using Android.Text;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.GenericSubdomains.Utils;

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
            Target.HintFormatted = Html.FromHtml("<small><i>{0}</i></small>".FormatString(value));
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}
