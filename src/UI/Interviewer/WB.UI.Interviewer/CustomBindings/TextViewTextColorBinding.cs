using Android.Graphics;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class TextViewTextColorBinding : BaseBinding<TextView, int?>
    {
        public TextViewTextColorBinding(TextView androidControl) : base(androidControl) { }

        protected override void SetValueToView(TextView control, int? colorId)
        {
            this.SetBackgroundDrawable(control, colorId);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void SetBackgroundDrawable(TextView androidControl, int? colorId)
        {
            if (colorId.HasValue)
            {
                Color color = androidControl.Resources.GetColor(colorId.Value);
                androidControl.SetTextColor(color); 
            }
        }
    }
}