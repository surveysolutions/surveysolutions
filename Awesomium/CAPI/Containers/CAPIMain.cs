using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using Browsing.Common.Containers;
using Browsing.Common.Controls;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Registration;

namespace Browsing.CAPI.Containers
{
    public partial class CAPIMain : Main
    {
        public CAPIMain(ISettingsProvider clientSettings, IRequestProcessor requestProcessor, IUrlUtils urlUtils, ScreenHolder holder)
            : base(clientSettings, requestProcessor, urlUtils, holder)
        {
            InitializeComponent();
        }

        protected override void OnRefreshRegistrationButton(bool userIsLoggedIn)
        {
            var regScreen = this.Holder.LoadedScreens.FirstOrDefault(s => s is CAPIRegistration) as CAPIRegistration;

            ChangeRegistrationButton(true, "Registration", regScreen == null ? null : regScreen.CurrentRegistrationStatus);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            new System.Threading.Thread(LookupSupervisor).Start();
        }

        private void LookupSupervisor()
        {
            IAuthorizationService channelService = null;
            string supervisorHost = "Not found";

            try
            {
                var discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
                var findCriteria = FindCriteria.CreateMetadataExchangeEndpointCriteria(typeof(IAuthorizationService));
                findCriteria.MaxResults = 1;

                var findResponse = discoveryClient.Find(findCriteria);
                if (findResponse == null || findResponse.Endpoints == null || findResponse.Endpoints.Count < 1)
                    return;

                var address = findResponse.Endpoints[0].Address;
                var endpoints = MetadataResolver.Resolve(typeof(IAuthorizationService), address);

                if (endpoints.Count < 1)
                    return;

                var factory = new ChannelFactory<IAuthorizationService>(endpoints[0].Binding, endpoints[0].Address);
                channelService = factory.CreateChannel();

                var channelResponce = channelService.GetPath();
                
                supervisorHost = endpoints[0].Address.ToString();
                supervisorHost = supervisorHost.Replace(channelResponce, "");

                Properties.Settings.Default.EndpointExportPath = supervisorHost;
                Properties.Settings.Default.Save();

                if (Holder.InvokeRequired)
                    Holder.Invoke(new System.Windows.Forms.MethodInvoker(() =>
                    {
                        Holder.UpdateConfigDependencies();
                    }));
            }
            catch
            {
            }
            finally
            {
                if (channelService != null)
                    ((IChannel)channelService).Close();

                if (Holder.InvokeRequired)
                    Holder.Invoke(new System.Windows.Forms.MethodInvoker(() =>
                    {
                        var textToShow = string.Format("{0} - supervisor runs by address: {1}", Holder.Parent.Text, supervisorHost);
                        Holder.Parent.Text = textToShow;
                    }));
            }
        }
    }
}
