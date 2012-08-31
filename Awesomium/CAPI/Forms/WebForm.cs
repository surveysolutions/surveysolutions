#region Using
using System;
using System.Linq;
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
using Browsing.CAPI.Containers;
using Browsing.CAPI.Properties;
using Browsing.CAPI.ClientSettings;
using Browsing.CAPI.Utils;
using Common.Utils;
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
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private Awesomium.Windows.Forms.WebControl webView;
        private ScreenHolder holder;
        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private IUrlUtils urlUtils;
        #region C-tor

        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            InitializeComponent();
            this.holder = new ScreenHolder();
            this.holder.Dock = DockStyle.Fill;

            WebCore.Initialize(new WebCoreConfig()
                                   {
                                       EnablePlugins = true,
                                       SaveCacheAndCookies = true
                                   }, true);

            this.webView = new WebControl();
            this.clientSettings = new ClientSettingsProvider();
            this.requestProcessor = new WebRequestProcessor();
            this.urlUtils = new UrlUtils();

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
            AddBrowser();
            AddSynchronizer();
            AddSettings();

            this.Controls.Add(this.holder);

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

        protected void AddBrowser()
        {
            new CAPIBrowser(this.webView, this.holder)
            {
                Name = "capiBrowser1"
            };
        }

        protected void AddSynchronizer()
        {
            new CAPISynchronization(this.clientSettings, this.requestProcessor, this.urlUtils, this.holder) 
            { 
                Name = "capiSync" 
            };
        }

        protected void AddSettings()
        {
            new CAPISettings(this.holder) 
            { 
                Name = "capiSettings" 
            };
        }

        protected void AddMain()
        {
            var capiMain = new CAPIMain(this.clientSettings, this.requestProcessor, this.urlUtils, this.holder)
            {
                Name = "capiMain"
            };

            this.holder.Redirect(capiMain);
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

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:

                    int n = (int)m.WParam;

                    if (n == DBT_DEVICEARRIVAL || n == DBT_DEVICEREMOVECOMPLETE)
                    {
                        var syncScreen = this.holder.LoadedScreens.FirstOrDefault(s => s is CAPISynchronization);
                        if(syncScreen != null)
                            (syncScreen as CAPISynchronization).UpdateUsbList();
                    }

                    break;
            }

            base.WndProc(ref m);
        }
    }
}