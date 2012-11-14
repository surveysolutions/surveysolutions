using Awesomium.Windows.Forms;
using Browsing.Common.Containers;
using Browsing.Supervisor.ClientSettings;
using Browsing.Supervisor.Containers;
using Browsing.Supervisor.Utils;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.Supervisor.Forms
{
    public partial class WebForm : Common.Forms.WebForm
    {
        #region Constructor

        public WebForm()
            : base(new ClientSettingsProvider())
        {
            InitializeComponent();

#if DEBUG__
            Properties.Settings.Default.RunClient = false;
            Properties.Settings.Default.DefaultUrl = "http://192.168.3.113/DevKharkiv-Supervisor/";
            Properties.Settings.Default.Save();
#endif
        }

        #endregion

        #region Overloaded

        protected override Common.Containers.Registration OnAddRegistrationScreen(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
        {
            return new SupervisorRegistration(requestProcessor, urlUtils, Holder)
            {
                Name = "supervisorRegistration"
            };
        }

        protected override Browser OnAddBrowserScreen(WebControl webView)
        {
            return new SupervisorBrowser(webView, Holder)
            {
                Name = "supervisorBrowser"
            };
        }

        protected override Common.Containers.Synchronization OnAddSynchronizerScreens(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            new SyncChoicePage(Holder)
            {
                Name = "supervisorSyncChoice"
            };

            new SyncHQProcessPage(settingsProvider, requestProcessor, urlUtils, Holder)
            {
                Name = "supervisorHQSync"
            };
            
            return new SyncCapiProcessPage(settingsProvider, requestProcessor, urlUtils, Holder)
            {
                Name = "supervisorSyncChoice"
            };
        }

        protected override Main OnAddMainPageScreen(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            return new SupervisorMain(settingsProvider, requestProcessor, urlUtils, Holder)
            {
                Name = "supervisorMain"
            };
        }

        protected override Common.Containers.Settings OnAddSettingsScreen()
        {
            return new SupervisorSettings(this.Holder)
            {
                Name = "supervisorSettings"
            };
        }

        protected override IUrlUtils InstantiateUrlProvider()
        {
            return new UrlUtils();
        }

        #endregion
    }
}
