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
using Browsing.Common;
using Browsing.Common.Containers;
using Common.Utils;
using Synchronization.Core.Events;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.SynchronizationFlow;
using Common;

#endif
#endregion

namespace Browsing.Common.Forms
{
    public abstract partial class WebForm : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        protected ScreenHolder Holder { get; private set; }

        #region C-tor

        public WebForm(ISettingsProvider settingsProvider)
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            InitializeComponent();

            this.Holder = new ScreenHolder();
            this.Holder.Dock = DockStyle.Fill;

            WebCore.Initialize(new WebCoreConfig()
                                   {
                                       EnablePlugins = true,
                                       SaveCacheAndCookies = true
                                   }, true);

            var webView = new WebControl();
            var requestProcessor = new WebRequestProcessor();

            var urlUtils = InstantiateUrlProvider();

            System.Diagnostics.Debug.Assert(urlUtils != null);

            if (settingsProvider.Settings.RunEngine)
            {
                try
                {
                    RunEngine(settingsProvider);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error on Engine Run. " + ex.Message);
                }

            }

            AddMain(requestProcessor, settingsProvider, urlUtils);
            AddBrowser(webView);
            AddSynchronizer(requestProcessor, settingsProvider, urlUtils);
            AddSettings();

            this.Controls.Add(this.Holder);

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

#if USING_MONO
    // TODO: Mac OS X: Sends a SIGSEGV to Mono.
            if ( !PlatformDetection.IsMac )
#endif
            WebCore.Shutdown();
        }

        private void AddBrowser(WebControl webView)
        {
            InstantiateBrowserContainer(webView);
        }

        private void AddSynchronizer(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            InstantiateSynchronizerContainer(requestProcessor, settingsProvider, urlUtils);
        }

        private void AddSettings()
        {
            InstantiateSettingsContainer();
        }

        private void AddMain(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            var capiMain = InstantiateMainPageContainer(requestProcessor, settingsProvider, urlUtils);
                
            this.Holder.Redirect(capiMain);
        }

        protected abstract IUrlUtils InstantiateUrlProvider();
        protected abstract Containers.Main InstantiateMainPageContainer(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils);
        protected abstract Containers.Settings InstantiateSettingsContainer();
        protected abstract Containers.Synchronization InstantiateSynchronizerContainer(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils);
        protected abstract Containers.Browser InstantiateBrowserContainer(WebControl webView);

        #endregion

        private string RunEngine(ISettingsProvider settingsProvider)
        {
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath);

            if (dir.Parent == null)
                throw new Exception("Engine was not found.");

            string enginePath = Path.Combine(dir.Parent.FullName, settingsProvider.Settings.EnginePathName);

            if (!Directory.Exists(enginePath))
                throw new Exception("Engine was not found.");

            string port = settingsProvider.Settings.DefaultPort;

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
                        var syncScreen = Holder.LoadedScreens.FirstOrDefault(s => s is Containers.Synchronization);
                        if(syncScreen != null)
                            (syncScreen as Containers.Synchronization).UpdateUsbList();
                    }

                    break;
            }

            base.WndProc(ref m);
        }
    }
}