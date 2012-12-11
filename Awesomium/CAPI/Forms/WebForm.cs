#region Using

#if USING_MONO
using Awesomium.Mono;
using Awesomium.Mono.Forms;
#else
using Awesomium.Windows.Forms;
using Browsing.CAPI.ClientSettings;
using Browsing.CAPI.Containers;
using Browsing.CAPI.Utils;
using Browsing.Common.Containers;
using Common.Utils;
using Synchronization.Core.Interface;


#endif
#endregion

namespace Browsing.CAPI.Forms
{
    public partial class WebForm : Browsing.Common.Forms.WebForm
    {
        #region C-tor

        public WebForm() :
            base(new ClientSettingsProvider())
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.
            InitializeComponent();

            #if DEBUG__
                        Properties.Settings.Default.RunClient = false;
                        Properties.Settings.Default.DefaultUrl = "http://localhost:8083/";
                        //Properties.Settings.Default.DefaultUrl = "http://192.168.3.113/DevKharkiv-CAPI/";
                        Properties.Settings.Default.Save();
            #endif
        }

        #endregion

        #region Overloaded

        protected override Common.Containers.Registration OnAddRegistrationScreen(IRequesProcessor requestProcessor, IUrlUtils urlUtils)
        {
            return new CAPIRegistration(requestProcessor,urlUtils, Holder)
            {
                Name = "capiRegistration"
            };
        }

        protected override Browser OnAddBrowserScreen()
        {
            return new CAPIBrowser(Holder)
                        {
                            Name = "capiBrowser1"
                        };
        }

        protected override Common.Containers.Synchronization OnAddSynchronizerScreens(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            return new CAPISynchronization(settingsProvider, requestProcessor, urlUtils, Holder)
            {
                Name = "capiSync"
            };
        }

        protected override Main OnAddMainPageScreen(IRequesProcessor requestProcessor, ISettingsProvider settingsProvider, IUrlUtils urlUtils)
        {
            return new CAPIMain(settingsProvider, requestProcessor, urlUtils, Holder)
            {
                Name = "capiMain"
            };
        }

        protected override Common.Containers.Settings OnAddSettingsScreen()
        {
            return new CAPISettings(this.Holder)
            {
                Name = "capiSettings"
            };
        }

        protected override IUrlUtils InstantiateUrlProvider()
        {
            return new UrlUtils();
        }

        #endregion

   }
}