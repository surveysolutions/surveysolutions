using System;
using System.Linq;
using Awesomium.Core;
using Common.Utils;
using Synchronization.Core.Interface;

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
        }
        protected void RefreshAuthentificationInfo()
        {
            isUserLoggedIn = null;
            this.btnDashboard.Enabled = btnLogout.Visible = IsUserLoggedIn;
            this.btnLogin.Visible = !IsUserLoggedIn;
        }

        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private bool? isUserLoggedIn;
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

        void btnSettings_Click(object sender, System.EventArgs e)
        {
            var settings = this.Holder.LoadedScreens.FirstOrDefault(s => s is Settings) as Settings;
            this.Holder.Redirect(settings);
            //new SettingsBox().ShowDialog();
        }

        void btnDashboard_Click(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is Browser) as Browser;
            browser.SetMode(false, urlUtils.GetDefaultUrl());
            this.Holder.Redirect(browser);
        }

        void btnSyncronization_Click(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is Synchronization));
        }


        private void btnLogout_Click(object sender, EventArgs e)
        {
            WebCore.ClearCookies();
            RefreshAuthentificationInfo();
        }

        void btnLogin_Click(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is Browser) as Browser;
            browser.SetMode(true, urlUtils.GetLoginUrl());
            this.Holder.Redirect(browser);
        }
        
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            RefreshAuthentificationInfo();
        }
    }
}
