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
using Common.Utils;
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : UserControl
    {
        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor)
        {
            InitializeComponent();
            this.clientSettings = clientSettings;
            this.requestProcessor = requestProcessor;
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
        protected string AuthentificationCheckUrl
        {
            get { return string.Format("{0}{1}", Settings.Default.DefaultUrl, Settings.Default.AuthentificationCheckPath); }
        }

        protected bool IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn.HasValue)
                    return isUserLoggedIn.Value;
                isUserLoggedIn = this.requestProcessor.Process<bool>(AuthentificationCheckUrl, "GET", true);
                return isUserLoggedIn.Value;
            }
        }

        void btnSettings_Click(object sender, System.EventArgs e)
        {
            new SettingsBox().ShowDialog();
        }
        void btnDashboard_Click(object sender, System.EventArgs e)
        {
            var handler = this.DashboardClick;
            if(handler!=null)
            {
                handler(this, e);
            }
        }

        void btnSyncronization_Click(object sender, System.EventArgs e)
        {
            var handler = this.SynchronizationClick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

       
        #region events

        public event EventHandler<EventArgs> DashboardClick;
        public event EventHandler<EventArgs> LoginClick;
        public event EventHandler<EventArgs> SynchronizationClick;
        
        #endregion

        private void btnLogout_Click(object sender, EventArgs e)
        {
            WebCore.ClearCookies();
            RefreshAuthentificationInfo();
        }
        void btnLogin_Click(object sender, System.EventArgs e)
        {
            var handler = this.LoginClick;
            if (handler != null)
                handler(this, e);
        }

    }
}
