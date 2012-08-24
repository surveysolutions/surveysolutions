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
using Synchronization.Core.Errors;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : UserControl
    {
        public CAPIMain(ISettingsProvider clientSettings)
        {
            InitializeComponent();
            this.clientSettings = clientSettings;
            RefreshAuthentificationInfo();
        }
      /*  protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            this.Left = (this.ClientSize.Width - this.Width) / 2;
            this.Top = (this.ClientSize.Height - this.Height) / 2;
     
        }*/
        protected void RefreshAuthentificationInfo()
        {
            isUserLoggedIn = null;
            this.btnDashboard.Enabled = btnLogout.Visible = IsUserLoggedIn;
            this.btnLogin.Visible = !IsUserLoggedIn;
        }

        private ISettingsProvider clientSettings;
        private bool? isUserLoggedIn;
        protected Uri AuthentificationCheckUrl
        {
            get { return new Uri(string.Format("{0}{1}", Settings.Default.DefaultUrl, Settings.Default.AuthentificationCheckPath)); }
        }

        protected bool IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn.HasValue)
                    return isUserLoggedIn.Value;
                try
                {

                    var cookies = WebCore.GetCookies(AuthentificationCheckUrl.ToString(), false);
                    if(string.IsNullOrEmpty(cookies))
                    {
                        isUserLoggedIn = false;
                        return isUserLoggedIn.Value;
                    }
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AuthentificationCheckUrl);
                    request.Method = "GET";
                    request.CookieContainer=new CookieContainer();
                    foreach (string cookie in cookies.Split(';'))
                    {
                        string name = cookie.Split('=')[0];
                        string value = cookie.Substring(name.Length + 1);
                        string path = "/";
                        string domain = AuthentificationCheckUrl.Host; //change to your domain name
                        request.CookieContainer.Add(new Cookie(name.Trim(), value.Trim(), path, domain));
                    }
                    // Get the response.
                    using (WebResponse response = request.GetResponse())
                    {
                        // Get the stream containing content returned by the server.
                        var dataStream = response.GetResponseStream();
                        // Open the stream using a StreamReader for easy access.
                        StreamReader reader = new StreamReader(dataStream);
                        // Read the content.
                        string responseFromServer = reader.ReadToEnd();

                        try
                        {
                            isUserLoggedIn   = Convert.ToBoolean(responseFromServer);

                        }
                        finally
                        {
                            // Clean up the streams.
                            reader.Close();
                            dataStream.Close();
                            response.Close();
                        }
                    }
                }
                catch (Exception e)
                {
                    isUserLoggedIn = false;
                }
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
