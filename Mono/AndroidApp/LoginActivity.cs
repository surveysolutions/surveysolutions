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
using AndroidApp.Controls.Navigation;
using AndroidApp.Core.Model.ViewModel.Login;
using AndroidApp.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;

namespace AndroidApp
{
    [Activity(Label = "CAPI", NoHistory = true, Icon = "@drawable/capi")]
    public class LoginActivity : MvxSimpleBindingActivity<LoginViewModel> /*, ActionBar.ITabListener*/
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel = new LoginViewModel(CapiApplication.Membership);
            if (CapiApplication.Membership.IsLoggedIn)
                StartActivity(typeof (DashboardActivity));
            SetContentView(Resource.Layout.Login);
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }
    }
}