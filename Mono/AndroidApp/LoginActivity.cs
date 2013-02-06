using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
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
            ViewModel = new LoginViewModel();
            if (CapiApplication.Membership.IsLoggedIn)
                StartActivity(typeof (DashboardActivity));
            SetContentView(Resource.Layout.Login);
            btnLogin.Click += btnLogin_Click;
        }

        void btnLogin_Click(object sender, EventArgs e)
        {
            var result = CapiApplication.Membership.LogOn(teLogin.Text, tePassword.Text);
            if (result)
            {
                StartActivity(typeof (DashboardActivity));
                return;
            }
            teLogin.SetBackgroundColor(Color.Red);
            tePassword.SetBackgroundColor(Color.Red);
        }

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
        }

        protected EditText teLogin
        {
            get { return FindViewById<EditText>(Resource.Id.teLogin); }
        }
        protected EditText tePassword
        {
            get { return FindViewById<EditText>(Resource.Id.tePassword); }
        }
        protected Button btnLogin
        {
            get { return FindViewById<Button>(Resource.Id.btnLogin); }
        }
    }
}