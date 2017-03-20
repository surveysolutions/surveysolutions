using System;
using System.Collections.Specialized;
using System.Web.Configuration;
using Ninject.Modules;
using WB.Core.Infrastructure.Aggregates;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Modules
{
    public class WebConfigurationModule : NinjectModule
    {
        private readonly NameValueCollection membershipSettings;
        public WebConfigurationModule(NameValueCollection membershipSettings = null)
        {
            this.membershipSettings = membershipSettings;
        }

        public override void Load()
        {
            Bind<IConfigurationManager>()
                .ToConstant(new ConfigurationManager(appSettings: WebConfigurationManager.AppSettings,
                    membershipSettings: this.membershipSettings));

            Bind(typeof(int)).ToMethod(context => 
                    Convert.ToInt32(WebConfigurationManager.AppSettings["MaxCachedAggregateRoots"]))
                .WhenInjectedInto<IEventSourcedAggregateRootRepository>();
        }
    }
}