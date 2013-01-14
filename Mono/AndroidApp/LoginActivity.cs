using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.Login;
using AndroidApp.ViewModel.Model;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace AndroidApp
{
    [Activity(Label = "CAPI", NoHistory = true, Icon = "@drawable/capi")]
    public class LoginActivity : MvxSimpleBindingActivity<LoginViewModel>, ActionBar.ITabListener
    {
        protected override void OnCreate(Bundle bundle)
        {
           
            base.OnCreate(bundle);
            ViewModel = new LoginViewModel();
            if (CapiApplication.Membership.IsLoggedIn)
                StartActivity(typeof(DashboardActivity));
            SetContentView(Resource.Layout.Login);

          
        }
        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {


            var res = base.OnCreateOptionsMenu(menu);

            ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;
           


            var surveyTab = ActionBar.NewTab();

            surveyTab.SetText("Dashboard");
            surveyTab.SetTabListener(this);
            surveyTab.SetTag(Resource.Id.ScreenId);
            ActionBar.AddTab(surveyTab);

            var syncTab = ActionBar.NewTab();

            syncTab.SetText("Synchronize");
            syncTab.SetTabListener(this);
            syncTab.SetTag(Resource.Id.QuestionnaireId);
            ActionBar.AddTab(syncTab);
            return res;

        }

        #region Implementation of ITabListener

        public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
        {

        }

     
        public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
        {
            if (int.Parse(tab.Tag.ToString()) == Resource.Id.QuestionnaireId)
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage("Sync");
                builder.Show();
            }
        }

        public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
        {
        }

        #endregion
    }
}