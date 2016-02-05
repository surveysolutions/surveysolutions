using Android.Graphics;
using Android.Widget;
using MvvmCross.Binding;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewTextColorBinding : BaseBinding<TextView, int?>
    {
        public TextViewTextColorBinding(TextView androidControl) : base(androidControl) { }

        protected override void SetValueToView(TextView control, int? colorId)
        {
            if (!colorId.HasValue)
                return;

            Color color = control.Resources.GetColor(colorId.Value);
            control.SetTextColor(color);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }
    }
}