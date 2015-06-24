using Android.Graphics.Drawables;
using Android.Views;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class ViewBackgroundDrawableBinding : BaseBinding<View, int?>
    {
        public ViewBackgroundDrawableBinding(View androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(View control, int? drawableBackgroundId)
        {
            SetBackgroundDrawable(control, drawableBackgroundId);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void SetBackgroundDrawable(View androidControl, int? backgroundId)
        {
            if (backgroundId.HasValue)
            {
                androidControl.SetBackgroundResource(backgroundId.Value);
            }
        }
    }
}