using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI.Properties;
using Common;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIBrowser : UserControl
    {
        public event EventHandler<EventArgs> HomeButtonClick; 
        public CAPIBrowser(WebControl webView)
        {
            this.webView = webView;
            InitializeComponent();
            string url;
            if (Settings.Default.RunClient)
            {
                try
                {
                    url = RunEngine();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on Engine Run. " + ex.Message);
                    url = Settings.Default.DefaultUrl;
                }

            }
            else
            {
                url = Settings.Default.DefaultUrl;
            }
            try
            {
                this.webView.Source = new Uri(Settings.Default.DefaultUrl);
                this.webView.Focus();
            }
            catch
            {
                // log
            }
        }

        void homeButton_Click(object sender, System.EventArgs e)
        {
            var handler = this.HomeButtonClick;
            if (handler != null)
                handler(this, e);
        }
        #region TODO Progress indication

        void webView_BeginLoading(object sender, BeginLoadingEventArgs e)
        {
            this.progressBox.Visible = true;
            this.panel1.Visible = e.Url == Settings.Default.DefaultUrl;
        }

        ResourceResponse webView_ResourceRequest(object sender, ResourceRequestEventArgs e)
        {
            return null;
        }

        void webView_LoadCompleted(object sender, EventArgs e)
        {
            this.progressBox.Visible = false;
        }

        #endregion

       
       
        private string RunEngine()
        {
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);

            if (dir.Parent == null)
                throw new Exception("Engine was not found.");

            string enginePath = Path.Combine(dir.Parent.FullName, Settings.Default.EnginePathName);

            if (!Directory.Exists(enginePath))
                throw new Exception("Engine was not found.");

            string port = Settings.Default.DefaultPort;

            EngineRunner runner = new EngineRunner();
            runner.RunEngine(enginePath, port);
            Application.ApplicationExit += runner.StopEngine;

            return String.Format("http://localhost:{0}", port);
        }
        
    }
}
