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

using Browsing.Common.Containers;

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
                        Properties.Settings.Default.DefaultUrl = "http://192.168.3.113/DevKharkiv-CAPI/";
                        Properties.Settings.Default.Save();
            #endif
        }

        #endregion

        #region Overloaded

        protected override Browser OnAddBrowserScreen(WebControl webView)
        {
            return new CAPIBrowser(webView, Holder)
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