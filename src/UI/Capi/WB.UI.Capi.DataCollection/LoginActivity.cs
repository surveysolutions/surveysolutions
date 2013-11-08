// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginActivity.cs" company="">
//   
// </copyright>
// <summary>
//   The login activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using Android.Content.PM;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.MvvmCross.Droid.Simple;
using WB.UI.Capi.DataCollection.Extensions;

namespace WB.UI.Capi.DataCollection
{
    /// <summary>
    /// The login activity.
    /// </summary>
    [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxSimpleBindingActivity//<LoginViewModel>
    {
        #region Properties

        /// <summary>
        /// Gets the btn login.
        /// </summary>
        protected Button btnLogin
        {
            get { return this.FindViewById<Button>(Resource.Id.btnLogin); }
        }

        /// <summary>
        /// Gets the te login.
        /// </summary>
        protected EditText teLogin
        {
            get { return this.FindViewById<EditText>(Resource.Id.teLogin); }
        }

        /// <summary>
        /// Gets the te password.
        /// </summary>
        protected EditText tePassword
        {
            get { return this.FindViewById<EditText>(Resource.Id.tePassword); }
        }

        #endregion

        #region Public Methods and Operators

        protected override void OnStart()
        {
            base.OnStart();
            this.CreateActionBar();

        }

        #endregion

        #region Methods

        /// <summary>
        /// The on create.
        /// </summary>
        /// <param name="bundle">
        /// The bundle.
        /// </param>
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

        /// <summary>
        /// The btn login_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            bool result = CapiApplication.Membership.LogOn(this.teLogin.Text, this.tePassword.Text);
            if (result)
            {
                this.ClearAllBackStack<DashboardActivity>();
             
                /*restore = () =>
                    {
                        CapiApplication.GenerateEvents(CapiApplication.Membership.CurrentUser.Id);
                        this.StartActivity(typeof (DashboardActivity));
                    };
                ProgressBar pb = new ProgressBar(this);
                this.AddContentView(pb,
                                    new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                               ViewGroup.LayoutParams.FillParent));
                restore.BeginInvoke(Callback, restore);
                */
                return;
            }

            this.teLogin.SetBackgroundColor(Color.Red);
            this.tePassword.SetBackgroundColor(Color.Red);
        }
/*
        private void Callback(IAsyncResult asyncResult)
        {
            Action asyncAction = (Action)asyncResult.AsyncState;
            asyncAction.EndInvoke(asyncResult);
        }

        private Action restore;*/

        #endregion
    }
}