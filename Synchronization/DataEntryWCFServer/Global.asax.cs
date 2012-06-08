using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using DataEntryWCFServer.App_Start;
using Ninject;
using SynchronizationMessages.Discover;

namespace DataEntryWCFServer
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            HostServices();
        }
        protected void HostServices()
        {
            string hostname = System.Net.Dns.GetHostName();
            var baseAddress = new UriBuilder("http", hostname, 80, "SpotSyncService");
            var h = new ServiceHost(typeof(SpotSyncService), baseAddress.Uri);
            // enable processing of discovery messages.  use UdpDiscoveryEndpoint to enable listening. use EndpointDiscoveryBehavior for fine control.
            h.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            h.AddServiceEndpoint(new UdpDiscoveryEndpoint());


            // create endpoint
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            h.AddServiceEndpoint(typeof(ISpotSync), binding, "");
            h.Open();
        }
        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}