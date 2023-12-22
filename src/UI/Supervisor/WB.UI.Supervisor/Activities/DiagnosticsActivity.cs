using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Supervisor.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, 
        Theme = "@style/GrayAppTheme",
        NoHistory = true,
        Exported = false)]
    public class DiagnosticsActivity : BaseActivity<DiagnosticsViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);

            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }
        
        public override bool OnSupportNavigateUp() {
            OnBackPressedDispatcher.OnBackPressed();
            return true;
        }

        protected override int ViewResourceId => Resource.Layout.diagnostics;
    }
}
