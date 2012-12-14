using System.Linq;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;

namespace Browsing.Supervisor.Containers
{
    public partial class SupervisorMain : Main
    {
        #region Constructor

        public SupervisorMain(ISettingsProvider clientSettings, IRequestProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        #endregion

        #region Override Methods

        protected override void OnRefreshRegistrationButton(bool userIsLoggedIn)
        {
            ChangeRegistrationButton(userIsLoggedIn, null);
        }

        protected override void OnSynchronizationClicked(object sender, System.EventArgs e)
        {
            this.Holder.Redirect(this.Holder.LoadedScreens.FirstOrDefault(s => s is SyncChoicePage));
        }

        #endregion

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);
            new System.Threading.Thread(ForceDiscoveryService).Start();
        }

        private void ForceDiscoveryService()
        {
            try
            {
                // just push the service to run
                var host = new AuthorizationServiceClient(UrlUtils.GetAuthServiceUrl()).GetPath(); 
            }
            catch
            { }
        }
    }
}
