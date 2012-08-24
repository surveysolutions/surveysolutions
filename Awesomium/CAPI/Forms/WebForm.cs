#region Using
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
#if USING_MONO
using Awesomium.Mono;
using Awesomium.Mono.Forms;
#else
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Browsing.CAPI;
using Browsing.CAPI.Properties;
using Browsing.CAPI.ClientSettings;
using Synchronization.Core.Events;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Browsing.CAPI.Synchronization;
using Common;

#endif
#endregion

namespace Browsing.CAPI.Forms
{
    public partial class WebForm : Form
    {
          private Containers.CAPIBrowser capiBrowser;
        // private Containers.CAPISynchronization capiSycn;
        //   private Containers.CAPIMain capiMain;
        private Awesomium.Windows.Forms.WebControl webView;
        private ISettingsProvider clientSettings;

        #region C-tor

        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            InitializeComponent();

            WebCore.Initialize(new WebCoreConfig()
                                   {
                                       EnablePlugins = true,
                                       SaveCacheAndCookies = true
                                   }, true);

            this.webView = new WebControl();
            this.clientSettings = new ClientSettingsProvider();
            string url;
            if (Settings.Default.RunClient)
            {
                try
                {
                    RunEngine();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on Engine Run. " + ex.Message);
                }

            }
            AddMain();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (this.webView != null)
            {
                //this.webView.IsDirtyChanged -= OnIsDirtyChanged;
                //this.webView.SelectLocalFiles -= OnSelectLocalFiles;                this.webView.Close();
            }

            base.OnFormClosed(e);

#if USING_MONO
    // TODO: Mac OS X: Sends a SIGSEGV to Mono.
            if ( !PlatformDetection.IsMac )
#endif
            WebCore.Shutdown();
        }

        protected void AddBrowser(bool isSinglePage)
        {
            if(this.capiBrowser==null)
                this.capiBrowser = new Browsing.CAPI.Containers.CAPIBrowser(this.webView);
            this.capiBrowser.SetMode(isSinglePage);
            this.capiBrowser.AutoSize = true;
            this.capiBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.capiBrowser.Name = "capiBrowser1";
            this.capiBrowser.HomeButtonClick += new EventHandler<EventArgs>(capiBrowser_HomeButtonClick);
            this.Controls.Add(this.capiBrowser);
        }

        protected void AddSynchronizer()
        {
            Containers.CAPISynchronization capiSycn =
                new Browsing.CAPI.Containers.CAPISynchronization(this.clientSettings);
            capiSycn.AutoSize = true;
            capiSycn.Dock = System.Windows.Forms.DockStyle.Fill;
            capiSycn.Name = "capiSync";
            capiSycn.BackClick += new EventHandler<EventArgs>(capiSycn_BackClick);
            this.Controls.Add(capiSycn);
        }

        protected void AddMain()
        {
            Containers.CAPIMain capiMain = new Browsing.CAPI.Containers.CAPIMain(this.clientSettings);
            capiMain.AutoSize = true;
            capiMain.Dock = System.Windows.Forms.DockStyle.Fill;
            capiMain.Name = "capiMain";
            this.Controls.Add(capiMain);
            capiMain.DashboardClick += new EventHandler<EventArgs>(capiMain_DashboardClick);
            capiMain.SynchronizationClick += new EventHandler<EventArgs>(capiMain_SynchronizationClick);
            capiMain.LoginClick += new EventHandler<EventArgs>(capiMain_LoginClick);
        }

        protected void ClearAll()
        {
            /* foreach (Control control in this.Controls)
             {
                 control.Dispose();
             }*/
            this.Controls.Clear();
        }

        private void capiSycn_BackClick(object sender, EventArgs e)
        {
            ClearAll();
            AddMain();
        }

        private void capiBrowser_HomeButtonClick(object sender, EventArgs e)
        {
            ClearAll();
            AddMain();
        }


        private void capiMain_LoginClick(object sender, EventArgs e)
        {
            //  this.tableLayoutPanel1.Controls
            ClearAll();
            AddBrowser(true);
        }

        private void capiMain_SynchronizationClick(object sender, EventArgs e)
        {
            ClearAll();
            AddSynchronizer();
        }

        private void capiMain_DashboardClick(object sender, EventArgs e)
        {
            ClearAll();
            AddBrowser(false);
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