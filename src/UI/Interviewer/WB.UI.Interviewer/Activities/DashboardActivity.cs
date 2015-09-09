using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Microsoft.Practices.ServiceLocation;
using Ninject;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Interviewer.Controls;
using WB.UI.Interviewer.CustomControls;
using WB.UI.Interviewer.Syncronization;
using WB.UI.Interviewer.ViewModel;
using WB.UI.Interviewer.ViewModel.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.CustomControls;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "", 
        Theme = "@style/GrayAppTheme", 
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class DashboardActivity : BaseActivity<DashboardViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.dashboard; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.SetSupportActionBar(this.FindViewById<Toolbar>(Resource.Id.toolbar));

            var recyclerView = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewsList);
            var layoutManager = new LinearLayoutManager(this);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.HasFixedSize = true;

            var adapter = new InterviewerDashboardAdapter(this, (IMvxAndroidBindingContext)this.BindingContext);
            recyclerView.Adapter = adapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.dashboard, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_synchronization:
                    this.ViewModel.SynchronizationCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(SettingsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;

            }
            return base.OnOptionsItemSelected(item);
        }

    }
}