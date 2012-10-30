#region Using
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

#if USING_MONO
using Awesomium.Mono;
using Awesomium.Mono.Forms;
#else
using Awesomium.Core;
using Awesomium.Windows.Forms;
using Common.Utils;
using System.IO;
using Synchronization.Core.Interface;
using Common;
using Browsing.Common.Controls;
using Synchronization.Core.Registration;

#endif
#endregion

namespace Browsing.Common.Forms
{
    public abstract partial class WebForm : Form
    {
        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVICEARRIVAL = 0x8000;
        private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

        private bool stopThread = false;
        protected ScreenHolder Holder { get; private set; }

        #region C-tor

        public WebForm(ISettingsProvider settingsProvider)
        {

            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            try
            {
                InitializeComponent();

                Thread splashThread = new Thread(new ThreadStart(SplashDisplayThread));
                splashThread.Start();

                this.Holder = new ScreenHolder();
                this.Holder.Dock = DockStyle.Fill;

                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                var assembly = Assembly.GetCallingAssembly();
                var cachePath = System.IO.Path.GetFileName(assembly.FullName);
                cachePath = cachePath.Remove(cachePath.IndexOf(','));
                cachePath = appDataFolder + "\\" + cachePath; // + "." + assembly.ImageRuntimeVersion;

                WebCore.Initialize(new WebCoreConfig()
                                       {
                                           EnablePlugins = true,
                                           SaveCacheAndCookies = true,
                                           UserDataPath = cachePath,
                                           LogLevel = Awesomium.Core.LogLevel.Normal,
                                           //.Verbose,
                                           //ForceSingleProcess = true
                                       }, true);

                var webView = new WebControl();
                var requestProcessor = new WebRequestProcessor();
                var rsaCryptoService= new RSACryptoService();
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

                //engine warm up
                new System.Threading.Thread(delegate()
                                                {
                                                    try
                                                    {
                                                        requestProcessor.Process(urlUtils.GetLoginUrl(), "False");
                                                    }
                                                    catch (Exception)
                                                    {
                                                        //throw;
                                                    }
                                                }).Start();

                AddMainScreen(requestProcessor, settingsProvider, rsaCryptoService, urlUtils);
                AddBrowserScreen(webView);
                AddSynchronizerScreens(requestProcessor, settingsProvider, urlUtils);
                AddSettingsScreen();

                this.Controls.Add(this.Holder);
            }
            finally
            {
                this.stopThread = true;
            }


        }

        #endregion

        #region Helpers


        private void SplashDisplayThread()
        {
            var splash = new SplashScreen();

            splash.Show();

            while (this.stopThread == false)
            {
                Application.DoEvents();
                Thread.Sleep(100);
            }
            splash.Hide();
        }

        private void AddBrowserScreen(WebControl webView)
        {
            OnAddBrowserScreen(webView);
        }

        private void AddSynchronizerScreens(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            OnAddSynchronizerScreens(requestProcessor, settingsProvider, urlUtils);
        }

        private void AddSettingsScreen()
        {
            OnAddSettingsScreen();
        }

        private void AddMainScreen(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider,IRSACryptoService rsaCryptoService, IUrlUtils urlUtils)
        {
            var capiMain = OnAddMainPageScreen(requestProcessor, settingsProvider, rsaCryptoService, urlUtils);

            this.Holder.Redirect(capiMain);
        }

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

        #endregion

        #region Abstract

        protected abstract IUrlUtils InstantiateUrlProvider();
        protected abstract Containers.Main OnAddMainPageScreen(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IRSACryptoService rsaCryptoService, IUrlUtils urlUtils);
        protected abstract Containers.Settings OnAddSettingsScreen();
        protected abstract Containers.Synchronization OnAddSynchronizerScreens(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils);
        protected abstract Containers.Browser OnAddBrowserScreen(WebControl webView);

        #endregion

        #region Overriden

        protected override void OnHandleDestroyed(EventArgs e)
        {
            this.stopThread = true;

            base.OnHandleDestroyed(e);
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

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:

                    int n = (int)m.WParam;

                    if (n == DBT_DEVICEARRIVAL || n == DBT_DEVICEREMOVECOMPLETE)
                    {
                        var syncScreen = Holder.LoadedScreens.Where(s => s is Containers.Synchronization);
                        if (syncScreen != null)
                        {
                            foreach (var screen in syncScreen)
                                (screen as Containers.Synchronization).UpdateUsbList();
                        }
                    }

                    break;
            }

            base.WndProc(ref m);
        }

        #endregion
    }
}