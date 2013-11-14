using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.MvvmCross.Droid.Simple;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.Capi.Extensions;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Capi
{
    /// <summary>
    /// The login activity.
    /// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxSimpleBindingActivity
    {
        protected Button btnLogin
        {
            get { return this.FindViewById<Button>(WB.UI.Capi.Resource.Id.btnLogin); }
        }
        protected EditText teLogin
        {
            get { return this.FindViewById<EditText>(WB.UI.Capi.Resource.Id.teLogin); }
        }
        protected EditText tePassword
        {
            get { return this.FindViewById<EditText>(WB.UI.Capi.Resource.Id.tePassword); }
        }

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }

        protected override void OnCreate(Bundle bundle)
        {
            this.DataContext = new LoginViewModel();
            base.OnCreate(bundle);
            if (CapiApplication.Membership.IsLoggedIn)
            {
                this.StartActivity(typeof (DashboardActivity));
            }

            this.SetContentView(Resource.Layout.Login);
            this.btnLogin.Click += this.btnLogin_Click;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool result = CapiApplication.Membership.LogOn(this.teLogin.Text, this.tePassword.Text);
            if (result)
            {
                this.ClearAllBackStack<DashboardActivity>();
                return;
            }

            this.teLogin.SetBackgroundColor(Color.Red);
            this.tePassword.SetBackgroundColor(Color.Red);
        }
    }
}