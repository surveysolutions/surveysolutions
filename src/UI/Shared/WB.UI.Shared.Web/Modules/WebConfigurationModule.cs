﻿using System.Collections.Specialized;
using System.Web.Configuration;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.Configuration;

namespace WB.UI.Shared.Web.Modules
{
    public class WebConfigurationModule : IModule
    {
        private readonly NameValueCollection membershipSettings;
        public WebConfigurationModule(NameValueCollection membershipSettings = null)
        {
            this.membershipSettings = membershipSettings;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant<IConfigurationManager>(() => new ConfigurationManager(
                appSettings: WebConfigurationManager.AppSettings,
                membershipSettings: this.membershipSettings)
            );
        }
    }
}