using Android.Views;
using Cirrious.MvvmCross.Binding;

namespace WB.UI.QuestionnaireTester.CustomBindings
{
    public class BackgroundDrawableBinding : BindingWrapper<View, int>
    {
        public BackgroundDrawableBinding(View androidControl)
            : base(androidControl)
        {
        }

        protected override void SetValueToView(View androidControl, int drawableBackgroundId)
        {
            SetBackgroundDrawable(androidControl, drawableBackgroundId);
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        }

        private void SetBackgroundDrawable(View androidControl, int questionBackgroundActive)
        {
            androidControl.SetBackgroundDrawable(androidControl.Resources.GetDrawable(questionBackgroundActive));
        }
    }
}