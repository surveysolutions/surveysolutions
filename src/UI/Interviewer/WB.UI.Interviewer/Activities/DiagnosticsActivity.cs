using Android.Views;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden,
        NoHistory = true,
        Theme = "@style/GrayAppTheme",
        Exported = false)]
    public class DiagnosticsActivity : BaseActivity<DiagnosticsViewModel>
    {
        public override void Finish()
        {
            base.Finish();
        }

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
        
        protected override int ViewResourceId => Resource.Layout.Diagnostics;
    }
}
