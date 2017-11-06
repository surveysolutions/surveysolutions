﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
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

        protected override int ViewResourceId
        {
            get { return Resource.Layout.Diagnostics; }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    break;
                case Resource.Id.menu_login:
                    this.ViewModel.NavigateToLoginCommand.Execute();
                    break;
                case Resource.Id.menu_maps:
                    this.ViewModel.NavigateToMapsCommand.Execute();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.diagnostics, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.menu_login, InterviewerUIResources.MenuItem_Title_Login);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);
            menu.LocalizeMenuItem(Resource.Id.menu_maps, InterviewerUIResources.MenuItem_Title_Maps);

            if (this.ViewModel.IsAuthenticated)
            {
                HideMenuItem(menu, Resource.Id.menu_login);
            }
            else
            {
                HideMenuItem(menu, Resource.Id.menu_dashboard);
                HideMenuItem(menu, Resource.Id.menu_signout);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public void HideMenuItem(IMenu menu, int menuItemId)
        {
            var menuItem = menu.FindItem(menuItemId);
            if (menuItem != null)
            {
                menuItem.SetVisible(false);
            }
        }
    }
}