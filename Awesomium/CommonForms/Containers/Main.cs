using System;
using System.Linq;
using Awesomium.Core;
using Common.Utils;
using Synchronization.Core.Interface;
using Browsing.Common.Controls;

namespace Browsing.Common.Containers
{
    public partial class Main : Screen
    {
        public Main(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(holder, false)
        {
            InitializeComponent();
            this.clientSettings = clientSettings;
            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            RefreshAuthentificationInfo();
            RefreshLoginCapability();
        }
        protected void RefreshAuthentificationInfo()
        {
            isUserLoggedIn = null;
            this.btnDashboard.Enabled = btnLogout.Visible = IsUserLoggedIn;
            this.btnLogin.Visible = !IsUserLoggedIn;
        }

        protected void RefreshLoginCapability()
        {
            isUsersInBase = null;
            this.btnLogin.Enabled = IsUsersInBase;
        }

        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private bool? isUserLoggedIn;
        private bool? isUsersInBase;
        private IUrlUtils urlUtils;

        protected bool IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn.HasValue)
                    return isUserLoggedIn.Value;
                isUserLoggedIn = this.requestProcessor.Process<bool>(urlUtils.GetAuthentificationCheckUrl(), "GET", true, false);
                return isUserLoggedIn.Value;
            }
        }

        protected bool IsUsersInBase
        {
            get
            {
                if (isUsersInBase.HasValue)
                    return isUsersInBase.Value;
                isUsersInBase = this.requestProcessor.Process<bool>(urlUtils.GetLoginCapabilitiesCheckUrl(), "GET", false, false);
                return isUsersInBase.Value;
            }
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

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            RefreshAuthentificationInfo();
        }
    }
}
