using Android.App;
using Android.Views;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
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