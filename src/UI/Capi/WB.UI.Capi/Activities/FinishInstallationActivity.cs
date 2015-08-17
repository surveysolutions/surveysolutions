using Android.App;
using Android.Views;
using WB.UI.Capi.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Capi.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
    public class FinishInstallationActivity : BaseActivity<FinishIntallationViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.FinishInstallation; }
        }
    }
}