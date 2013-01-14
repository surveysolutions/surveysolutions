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
    public class LoginActivity : MvxSimpleBindingActivity<LoginViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel = new LoginViewModel();
            if (CapiApplication.Membership.IsLoggedIn)
                StartActivity(typeof(DashboardActivity));
            SetContentView(Resource.Layout.Login);
        }
    }
}