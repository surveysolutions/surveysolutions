using System;
using System.Linq;
using System.Windows.Forms;
using Awesomium.Core;
using Common.Utils;
using Synchronization.Core.Interface;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class Main : Screen
    {
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
        }

        #endregion

        #region Helpers

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
        }

        private void RefreshAuthentificationInfo()
        {
            isDatabaseContainsUsers = null;
            isUserLoggedIn = null;

            IntitLogControls(false, false);

            new System.Threading.Thread(() => 
                {
                    if (this.IsDisposed)
                        return;

                    var userIsLoggedIn = IsUserLoggedIn;
                    var loginIsPossible = userIsLoggedIn || IsDatabaseContainsUsers; // minimize web-application access

                    this.Invoke(new MethodInvoker(() => IntitLogControls(userIsLoggedIn, loginIsPossible)));
                });
        }

        #endregion

        #region Private Members
        
        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private bool? isUserLoggedIn;
        private bool? isDatabaseContainsUsers;
        private IUrlUtils urlUtils;

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

        #endregion

        #region Handlers

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

        protected virtual void OnExitClicked(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        #endregion

        #region Overloading

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            RefreshAuthentificationInfo();
        }

        #endregion
    }
}
