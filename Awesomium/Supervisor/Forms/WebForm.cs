using System;
using Common;
using System.IO;
using Common.Utils;
using Awesomium.Core;
using System.Windows.Forms;
using Awesomium.Windows.Forms;
using Browsing.Supervisor.Utils;
using Browsing.Supervisor.Controls;
using Browsing.Supervisor.Containers;
using Browsing.Supervisor.Properties;
using Synchronization.Core.Interface;
using Browsing.Supervisor.ClientSettings;

namespace Browsing.Supervisor.Forms
{
    public partial class WebForm : Form
    {
        #region Properties

        private WebControl webView;
        private ScreenHolder holder;
        private ISettingsProvider clientSettings;
        private IRequesProcessor requestProcessor;
        private IUrlUtils urlUtils;

        #endregion

        #region Constructor

        public WebForm()
        {
            InitializeComponent();
            this.holder = new ScreenHolder {Dock = DockStyle.Fill};
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
            AddSyncHQPage();
            AddSyncCapiPage();
            this.Controls.Add(this.holder);
        }

        #endregion

        #region Methods

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            WebCore.Shutdown();
        }

        protected void AddBrowser()
        {
            new BrowserPage(this.webView, this.holder) { Name = "supervisorBrowser" };
        }


        protected void AddSettings()
        {
            new SettingsPage(this.holder) { Name = "supervisorSettings" };
        }

        protected void AddSynchronizer()
        {
            new SynchronizationPage(this.holder) { Name = "supervisorSync" };
        }

        protected void AddMain()
        {
            var pageMain = new MainPage(this.clientSettings, this.requestProcessor, this.urlUtils, this.holder)
                                    {Name = "supervisorMain"};
            this.holder.Redirect(pageMain);
        }

        protected void AddSyncHQPage()
        {
            new SyncHQProcessPage(this.clientSettings, this.requestProcessor, this.urlUtils, this.holder)
                { Name = "SynchronizationHQ" };
        }

        protected void AddSyncCapiPage()
        {
            new SyncCapiProcessPage(this.clientSettings, this.requestProcessor, this.urlUtils, this.holder) { Name = "SynchronizationCapi" };
        }

        private string RunEngine()
        {
            var dir = new DirectoryInfo(Application.StartupPath);
            if (dir.Parent == null)
                throw new Exception("Engine was not found.");
            string enginePath = Path.Combine(dir.Parent.FullName, Settings.Default.EnginePathName);
            if (!Directory.Exists(enginePath))
                throw new Exception("Engine was not found.");
            string port = Settings.Default.DefaultPort;
            var runner = new EngineRunner();
            runner.RunEngine(enginePath, port);
            Application.ApplicationExit += runner.StopEngine;
            return String.Format("http://localhost:{0}", port);
        }

        #endregion
    }
}
