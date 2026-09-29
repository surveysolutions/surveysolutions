using Android.Widget;
using MvvmCross.Binding;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Supervisor.MvvmBindings
{
    public class ImageCompanyLogoBinding : ImageViewBitmapWithFallbackBinding
    {
        public ImageCompanyLogoBinding(ImageView androidControl) : base(androidControl)
        {
        }

        protected override void SetDefaultImage(ImageView control)
        {
            control.SetImageResource(Resource.Drawable.login_logo);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;
    }
}
