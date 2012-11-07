using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Awesomium.Core;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.Common.Containers
{
    public abstract partial class Main : Screen
    {
        private bool destroyed = false;
        private bool checkIsRunning = false;
        
        #region C-tor

        public Main(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(holder, false)
        {
            InitializeComponent();
            this.clientSettings = clientSettings;
            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;

            IntitLogControls(false, false);

            //RefreshAuthentificationInfo();

            this.statusLabel.ForeColor = System.Drawing.Color.Red;
            this.regLabel.ForeColor = System.Drawing.Color.Red;
        }

        #endregion

        #region Protected Methods

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.destroyed = true;
            base.OnHandleDestroyed(e);
        }

        protected Guid GetCurrentUser()
        {
            return CurrentUser;
        }

        protected void ChangeRegistrationButton(bool enabled, string text, string label = null)
        {
            this.btnRegistration.Enabled = enabled;
            
            if (!String.IsNullOrEmpty(text))
                this.btnRegistration.Text = text;

            if (!String.IsNullOrEmpty(label))
                this.regLabel.Text = label;
        }

        #endregion
        

        #region Helpers

        /// <summary>
        /// TODO: remove upon registration page has been implemented
        /// </summary>
        /// <returns></returns>
        protected DriveInfo GetUsbDrive()
        {
            List<DriveInfo> drivers = new List<DriveInfo>();
            DriveInfo[] listDrives = DriveInfo.GetDrives();

            foreach (var drive in listDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                    return drive;
            }

            return null;
        }

        private void SetCheckingStatus(bool? checking)
        {
            this.statusLabel.Text = checking.HasValue ?
                (checking.Value ? "Checking possibility to log in. Please, wait .." : "Please, synchronize your data to log in") 
                : string.Empty;
        }

        /// <summary>
        /// Assign access properties to Login, Logout and Dashboard buttons
        /// </summary>
        /// <param name="userIsLogged"></param>
        /// <param name="loginIsPossible"></param>
        private void IntitLogControls(bool userIsLoggedIn, bool loginIsPossible)
        {
            this.btnDashboard.Enabled = this.btnLogout.Visible = userIsLoggedIn;
            this.btnLogin.Visible = !userIsLoggedIn;
            this.btnLogin.Enabled = loginIsPossible;

            if(loginIsPossible)
                SetCheckingStatus(null);
            else
                SetCheckingStatus(false);
        }

        private void RefreshRegistrationButton(bool userIsLoggedIn)
        {
            OnRefreshRegistrationButton(userIsLoggedIn);
        }

        private void RefreshAuthentificationInfo()
        {
            isDatabaseContainsUsers = null;
            isUserLoggedIn = null;

            var userIsLoggedIn = IsUserLoggedIn; // cannot be accessed in extra thread; exception in WebCore.GetCookies

            IntitLogControls(userIsLoggedIn, userIsLoggedIn);

            RefreshRegistrationButton(userIsLoggedIn);

            this.statusLabel.Text = string.Empty;

            if (userIsLoggedIn)
                return;

            SetCheckingStatus(true);

            new System.Threading.Thread(() =>
                {
                    if (this.checkIsRunning)
                        return;

                    this.checkIsRunning = true;

                    try
                    {
                        bool loginIsPossible = false;
                        for (int tries = 0; !loginIsPossible; tries++)
                        {
                            if (this.IsDisposed)
                                return;

                            loginIsPossible = IsDatabaseContainsUsers; // minimize web-application access

                            while (!this.destroyed && !this.IsHandleCreated)
                                Thread.Sleep(300);

                            if (this.destroyed)
                                return;

                            if (loginIsPossible || tries > 10) // todo: let's tune the number
                                this.Invoke(new MethodInvoker(() => IntitLogControls(false, loginIsPossible)));

                            if (loginIsPossible)
                                return;

                            Thread.Sleep(1000);

                            if (this.destroyed)
                                return;
                            this.Invoke(new MethodInvoker(() => SetCheckingStatus(true)));

                            this.isDatabaseContainsUsers = null; // reset
                        };
                    }
                    finally
                    {
                        this.checkIsRunning = false;
                    }

                }).Start();
        }

        #endregion

        #region Private Members

        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private bool? isUserLoggedIn;
        private bool? isDatabaseContainsUsers;
        private Guid? currentUser;
        protected IUrlUtils urlUtils;

        #endregion

        #region Helper Properties

        private bool IsUserLoggedIn
        {
            get
            {
                if (this.isUserLoggedIn.HasValue)
                    return this.isUserLoggedIn.Value;

                this.isUserLoggedIn = this.requestProcessor.Process<bool>(urlUtils.GetAuthentificationCheckUrl(), "GET", true, false);
                return isUserLoggedIn.Value;
            }
        }

        private bool IsDatabaseContainsUsers
        {
            get
            {
                if (this.isDatabaseContainsUsers.HasValue)
                    return this.isDatabaseContainsUsers.Value;

                this.isDatabaseContainsUsers = this.requestProcessor.Process<bool>(urlUtils.GetLoginCapabilitiesCheckUrl(), "GET", false, false);
                return this.isDatabaseContainsUsers.Value;
            }
        }

        private Guid CurrentUser
        {
            get
            {
                if (this.currentUser.HasValue)
                    return this.currentUser.Value;

                this.currentUser = this.requestProcessor.Process<Guid>(urlUtils.GetCurrentUserGetUrl(), "GET", true, Guid.Empty);
                
                return this.currentUser.Value;
            }
        }

        #endregion

        #region Handlers

        private void btnRegistration_Click(object sender, EventArgs e)
        {
            OnRegistrationClicked(sender, e);
        }

        void btnSettings_Click(object sender, System.EventArgs e)
        {
            OnSettingsClicked(sender, e);
        }

        void btnDashboard_Click(object sender, System.EventArgs e)
        {
            OnDashboardClicked(sender, e);
        }

        void btnSyncronization_Click(object sender, System.EventArgs e)
        {
            OnSynchronizationClicked(sender, e);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            OnLogoutClicked(sender, e);
        }

        void btnLogin_Click(object sender, System.EventArgs e)
        {
            OnLoginClicked(sender, e);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            OnExitClicked(sender, e);
        }

        #endregion

        #region Virtual operations

        //protected internal abstract void AddRegistrationButton(TableLayoutPanel tableLayoutPanel);

        protected virtual void OnSynchronizationClicked(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is Synchronization));
        }

        protected virtual void OnLoginClicked(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is Browser) as Browser;
            browser.SetMode(true, urlUtils.GetLoginUrl());
            this.Holder.Redirect(browser);
        }

        protected virtual void OnLogoutClicked(object sender, System.EventArgs e)
        {
            WebCore.ClearCookies();
            RefreshAuthentificationInfo();
        }

        protected virtual void OnDashboardClicked(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is Browser) as Browser;
            browser.SetMode(false, urlUtils.GetDefaultUrl());
            this.Holder.Redirect(browser);
        }

        protected virtual void OnSettingsClicked(object sender, System.EventArgs e)
        {
            var settings = this.Holder.LoadedScreens.FirstOrDefault(s => s is Settings) as Settings;
            this.Holder.Redirect(settings);
        }

        protected virtual void OnRegistrationClicked(object sender, System.EventArgs e)
        {
            var registration = this.Holder.LoadedScreens.FirstOrDefault(s => s is Registration) as Registration;
            this.Holder.Redirect(registration);
        }
      
        protected virtual void OnExitClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        protected abstract void OnRefreshRegistrationButton(bool userIsLoggedIn);

        #endregion

        #region Overloading

        protected override void OnValidateContent()
        {
            base.OnValidateContent();

            RefreshAuthentificationInfo();
        }

        #endregion
    }
}
