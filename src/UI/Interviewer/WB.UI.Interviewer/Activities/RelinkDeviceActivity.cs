using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme", NoHistory = true)]
    public class RelinkDeviceActivity : BaseActivity<RelinkDeviceViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.relink; }
        }
        public override void OnBackPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }
    }
}