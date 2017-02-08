using System;
using System.Web.Configuration;
using Ninject.Modules;
using WB.Core.Infrastructure.Aggregates;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Modules
{
    public class WebConfigurationModule : NinjectModule
    {
        public override void Load()
        {
            var membershipSection = (MembershipSection)WebConfigurationManager.GetSection("system.web/membership");
            var membershipSettings = membershipSection.Providers[membershipSection.DefaultProvider].Parameters;

            Bind<IConfigurationManager>()
                .ToConstant(new ConfigurationManager(appSettings: WebConfigurationManager.AppSettings,
                    membershipSettings: membershipSettings));

            Bind(typeof(int)).ToMethod(context => 
                    Convert.ToInt32(WebConfigurationManager.AppSettings["MaxCachedAggregateRoots"]))
                .WhenInjectedInto<IEventSourcedAggregateRootRepository>();
        }
    }
}