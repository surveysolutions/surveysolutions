using Android.App;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Widget;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Callbacks;
using Toolbar=AndroidX.AppCompat.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden,
        Theme = "@style/GrayAppTheme",
        NoHistory = true,
        Exported = false)]
    public class RelinkDeviceActivity : BaseActivity<RelinkDeviceViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.relink;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }
        
        protected override bool BackButtonCustomAction => true;
        protected override void BackButtonPressed()
        {
            this.ViewModel.NavigateToPreviousViewModel();
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.relink, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, EnumeratorUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
