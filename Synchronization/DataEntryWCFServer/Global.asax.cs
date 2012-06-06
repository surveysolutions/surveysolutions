using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Discovery;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Wcf;
using Ninject.Parameters;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Conventions;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.Subscribers;
using RavenQuestionnaire.Core.ExpressionExecutors;
using SynchronizationMessages.Discover;
using SynchronizationMessages.Handshake;

namespace DataEntryWCFServer
{
    public class Global : NinjectWcfApplication
    {
        protected override void Application_Start(object sender, EventArgs e)
        {
            base.Application_Start(sender, e);
            HostServices(this.Kernel);
        }

        protected void HostServices(IKernel kernel)
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

      /*  protected void WcfTestHost_Open()
        {
            string hostname = System.Environment.MachineName;
            var baseAddress = new UriBuilder("http", hostname, 7400, "WcfPing");
            var h = new ServiceHost(typeof(WcfPingTestService), baseAddress.Uri);

            // enable processing of discovery messages.  use UdpDiscoveryEndpoint to enable listening. use EndpointDiscoveryBehavior for fine control.
            h.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            h.AddServiceEndpoint(new UdpDiscoveryEndpoint());

            // enable wsdl, so you can use the service from WcfStorm, or other tools.
            var smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            h.Description.Behaviors.Add(smb);

            // create endpoint
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            h.AddServiceEndpoint(typeof(IWcfPingTest), binding, "");
            h.Open();
            Console.WriteLine("host open");
        }*/
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

        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel(new CoreRegistry(System.Web.Configuration.WebConfigurationManager.AppSettings["Raven.DocumentStore"]));
            RegisterServices(kernel);
            
       //     kernel.Bind<ServiceHost>().ToMethod(ctx => ctx.Kernel.Get<NinjectServiceHost>(new ConstructorArgument("singletonInstance", c => c.Kernel.Get<IGetLastSyncEvent>())));
      //      HostServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private void RegisterServices(IKernel kernel)
        {

            
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(ICommandHandler<>)));
            });

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IViewFactory<,>)));
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IExpressionExecutor<,>)));
            });

            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new RegisterFirstInstanceOfInterface());
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(Iterator<>)));
            });
            kernel.Scan(s =>
            {
                s.FromAssembliesMatching("RavenQuestionnaire.*");
                s.BindWith(new GenericBindingGenerator(typeof(IEntitySubscriber<>)));
            });
        }
    }
}