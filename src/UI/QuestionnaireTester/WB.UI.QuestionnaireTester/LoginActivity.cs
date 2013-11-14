using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Cirrious.MvvmCross.Droid.Simple;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.UI.QuestionnaireTester.Services;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.QuestionnaireTester
{
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxSimpleBindingActivity
    {
        private ProgressDialog progressDialog;
        private CancellationToken cancellationToken;

        private int loginClickCount = 0;
        private int passwordClickCount = 0;
        private const int RequiredAmountOfClick = 5;
        private const string DesignerPath = "DesignerPath";

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

        protected LinearLayout llDesignerPath
        {
            get { return this.FindViewById<LinearLayout>(Resource.Id.llDesignerPath); }
        }

        protected EditText tePathToDesigner
        {
            get { return this.FindViewById<EditText>(Resource.Id.tePathToDesigner); }
        }

        protected Button btnSave
        {
            get { return this.FindViewById<Button>(Resource.Id.btnSave); }
        }

        protected ScrollView topView
        {
            get { return this.FindViewById<ScrollView>(Resource.Id.topView); }
        }

        protected override void OnCreate(Bundle bundle)
        {

            this.DataContext = new LoginViewModel();
            base.OnCreate(bundle);

            this.progressDialog = new ProgressDialog(this);

            this.progressDialog.SetTitle("Processing");
            this.progressDialog.SetMessage("Verifing user name and password");
            this.progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            this.progressDialog.SetCancelable(false);

            if (CapiTesterApplication.Membership.IsLoggedIn)
            {
                this.StartActivity(typeof(QuestionnaireListActivity));
            }

            this.SetContentView(Resource.Layout.Login);
            this.btnLogin.Click += this.btnLogin_Click;
            this.teLogin.Click += editText_Click;
            this.tePassword.Click += editText_Click;
            this.btnSave.Click += btnSave_Click;
            this.topView.Click += topView_Click;
        }

        void topView_Click(object sender, EventArgs e)
        {
            loginClickCount = passwordClickCount = 0;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            loginClickCount = passwordClickCount = 0;

            CapiTesterApplication.SetPathToDesigner(tePathToDesigner.Text.Trim());
            
            this.HidePathPanel();
        }

        private void editText_Click(object sender, EventArgs e)
        {
            if (sender == teLogin)
                loginClickCount++;
            if (sender == tePassword)
                passwordClickCount++;
            this.ShowPathPanelIfAllowed();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var tokenSource2 = new CancellationTokenSource();
            this.cancellationToken = tokenSource2.Token;
            progressDialog.Show();
            Task.Factory.StartNew(LoginAsync, this.cancellationToken);
        
        }

        private void LoginAsync()
        {
            bool result = CapiTesterApplication.Membership.LogOn(this.teLogin.Text, this.tePassword.Text, cancellationToken);
            this.RunOnUiThread(() =>
            {
                progressDialog.Hide();
                if (result)
                {
                    this.teLogin.Text = this.tePassword.Text = string.Empty;
                    this.ClearAllBackStack<QuestionnaireListActivity>();
                    return;
                }

                this.teLogin.SetBackgroundColor(Color.Red);
                this.tePassword.SetBackgroundColor(Color.Red);
            });
        }

        private void ShowPathPanelIfAllowed()
        {
            if (loginClickCount < RequiredAmountOfClick || passwordClickCount < RequiredAmountOfClick)
                return;
            tePathToDesigner.Text = CapiTesterApplication.GetPathToDesigner();
            llDesignerPath.Visibility = ViewStates.Visible;
        }

        private void HidePathPanel()
        {
            llDesignerPath.Visibility = ViewStates.Gone;
            
        }
    }
}