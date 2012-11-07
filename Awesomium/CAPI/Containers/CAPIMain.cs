using System.Collections.Generic;
using System.Linq;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Main
    {
        public CAPIMain(ISettingsProvider clientSettings, IRequesProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        protected override void OnRefreshRegistrationButton(bool userIsLoggedIn)
        {
            var regScreen = this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIRegistration) as CAPIRegistration;

            ChangeRegistrationButton(true, "Registration", regScreen == null ? null : regScreen.CurrentRegistrationStatus);
        }
    }
}
