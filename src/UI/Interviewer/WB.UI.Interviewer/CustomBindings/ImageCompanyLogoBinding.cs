using Android.Graphics;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;
using MvvmCross.Binding;
using MvvmCross.Platform.Droid.Platform;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class ImageCompanyLogoBinding : ImageViewBitmapWithFallbackBinding
    {
        private Bitmap nullImageBitmap;
        public ImageCompanyLogoBinding(ImageView androidControl) : base(androidControl)
        {
        }

        protected override void SetDefaultImage(ImageView control)
        {
            if (nullImageBitmap == null)
            {
                var mvxAndroidCurrentTopActivity = ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>();
                var resources = mvxAndroidCurrentTopActivity.Activity.Resources;
                var noImageOptions = new BitmapFactory.Options { InPurgeable = true };
                nullImageBitmap = BitmapFactory.DecodeResource(resources, Resource.Drawable.login_logo, noImageOptions);
            }

            control.SetImageBitmap(nullImageBitmap);
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneTime;
    }
}