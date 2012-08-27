using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Awesomium.Core;
using Browsing.CAPI.Forms;
using Browsing.CAPI.Properties;
using Browsing.CAPI.Utils;
using Common.Utils;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Screen
    {
        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(holder)
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
            new SettingsBox().ShowDialog();
        }
        void btnDashboard_Click(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIBrowser) as CAPIBrowser;
            browser.SetMode(false, urlUtils.GetDefaultUrl());
            this.Holder.Redirect(browser);
        }

        void btnSyncronization_Click(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPISynchronization));
        }


        private void btnLogout_Click(object sender, EventArgs e)
        {
            WebCore.ClearCookies();
            RefreshAuthentificationInfo();
        }
        void btnLogin_Click(object sender, System.EventArgs e)
        {
            var browser = this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIBrowser) as CAPIBrowser;
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
