using Android.Graphics;
using Android.Widget;
using AndroidX.Core.Content;
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

            var color = new Color(ContextCompat.GetColor(control.Context, colorId.Value));
            control.SetTextColor(color);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;
    }
}
