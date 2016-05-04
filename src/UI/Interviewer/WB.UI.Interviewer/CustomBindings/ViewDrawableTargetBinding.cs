using Android.Views;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class ViewDrawableTargetBinding : BaseBinding<View, int>
    {
        public ViewDrawableTargetBinding(View androidControl) : base(androidControl) { }

        protected override void SetValueToView(View control, int drawableId)
        {
            this.SetBackgroundDrawable(control, drawableId);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void SetBackgroundDrawable(View androidControl, int drawableId)
        {
                androidControl.SetBackgroundResource(drawableId);    
        }
    }
}