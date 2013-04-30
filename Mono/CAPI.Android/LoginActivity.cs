// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoginActivity.cs" company="">
//   
// </copyright>
// <summary>
//   The login activity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Login;
using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using CAPI.Android.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Simple;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot.EventStores;
using Ninject;

namespace CAPI.Android
{
    using global::Android.Content.PM;

    /// <summary>
    /// The login activity.
    /// </summary>
    [Activity(NoHistory = true, Icon = "@drawable/capi",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class LoginActivity : MvxSimpleBindingActivity<LoginViewModel> /*, ActionBar.ITabListener*/
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

        /// <summary>
        /// The on create options menu.
        /// </summary>
        /// <param name="menu">
        /// The menu.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.CreateActionBar();
            return base.OnCreateOptionsMenu(menu);
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
            base.OnCreate(bundle);
            this.ViewModel = new LoginViewModel();
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
                this.StartActivity(typeof(DashboardActivity));
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