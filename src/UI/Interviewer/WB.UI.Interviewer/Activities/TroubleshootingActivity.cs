using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Interviewer.Controls;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
    public class TroubleshootingActivity : BaseActivity<TroubleshootingViewModel>
    {
        private CancellationTokenSource cancelSource;

        protected EventHandler<EventArgs> versionCheckEventHandler;

        protected TabletInformationReportButton btnSendTabletInfo
        {
            get { return this.FindViewById<TabletInformationReportButton>(Resource.Id.btnSendTabletInfo); }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        private int clickCount = 0;
        const int NUMBER_CLICK=10;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            GC.Collect();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.settings, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override int ViewResourceId
        {
            get { return Resource.Layout.Troubleshooting; }
        }
    }
}