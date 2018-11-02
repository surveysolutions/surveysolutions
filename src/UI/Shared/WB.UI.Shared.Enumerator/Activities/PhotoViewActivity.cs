using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/AppTheme", 
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Exported = false)]
    public class PhotoViewActivity : BaseActivity<PhotoViewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_photo_view;
        
        public override void OnBackPressed()
        {
            this.Cancel();
        }

        private void Cancel()
        {
            this.Finish();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }
    }
}
