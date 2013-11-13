using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Simple;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxSimpleBindingActivity
    {
        protected Button btnLogin
        {
            get { return this.FindViewById<Button>(Resource.Id.btnLogin); }
        }

        protected EditText teLogin
        {
            get { return this.FindViewById<EditText>(Resource.Id.teLogin); }
        }

        protected EditText tePassword
        {
            get { return this.FindViewById<EditText>(Resource.Id.tePassword); }
        }

        protected override void OnCreate(Bundle bundle)
        {

            this.DataContext = new LoginViewModel();
            base.OnCreate(bundle);

            if (CapiTesterApplication.Membership.IsLoggedIn)
            {
                this.StartActivity(typeof(QuestionnaireListActivity));
            }

            this.SetContentView(Resource.Layout.Login);
            this.btnLogin.Click += this.btnLogin_Click;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool result = CapiTesterApplication.Membership.LogOn(this.teLogin.Text, this.tePassword.Text);
            if (result)
            {
                this.ClearAllBackStack<QuestionnaireListActivity>();
                return;
            }

            this.teLogin.SetBackgroundColor(Color.Red);
            this.tePassword.SetBackgroundColor(Color.Red);
        }
    }
}