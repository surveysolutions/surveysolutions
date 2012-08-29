using System;
using System.Linq;
using Common.Utils;
using Awesomium.Core;
using Browsing.Supervisor.Forms;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Containers
{
    public partial class MainPage : Screen
    {

        #region Properties

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

        #endregion

        #region Constructor

        public MainPage(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(holder)
        {
            InitializeComponent();
            this.clientSettings = clientSettings;
            this.requestProcessor = requestProcessor;
            this.urlUtils = urlUtils;
            RefreshAuthentificationInfo();
        }

        #endregion

        #region Methods

        protected void RefreshAuthentificationInfo()
        {
            isUserLoggedIn = null;
            this.btnDashboard.Enabled = btnLogout.Visible = IsUserLoggedIn;
            this.btnLogin.Visible = !IsUserLoggedIn;
        }

        void btnSettings_Click(object sender, EventArgs e)
        {
            new SettingsBox().ShowDialog();
        }

        void btnDashboard_Click(object sender, EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is BrowserPage) as BrowserPage;
            browser.SetMode(false, urlUtils.GetDefaultUrl());
            this.Holder.Redirect(browser);
        }


        void btnSyncronization_Click(object sender, EventArgs e)
        {
            //this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPISynchronization));
        }
        
        private void btnLogout_Click(object sender, EventArgs e)
        {
            WebCore.ClearCookies();
            RefreshAuthentificationInfo();
        }

        void btnLogin_Click(object sender, EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is BrowserPage) as BrowserPage;
            browser.SetMode(true, urlUtils.GetLoginUrl());
            this.Holder.Redirect(browser);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            RefreshAuthentificationInfo();
        }

        #endregion
    }
}
