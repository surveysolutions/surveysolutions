using System;
using System.Net.Http;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.AspNetCore;

namespace WB.UI.Headquarters.Services.Impl
{
    internal class DesignerApiConfigurator : IHttpClientConfigurator<IDesignerApi>
    {
        private readonly IRestServiceSettings serviceSettings;
        
        public DesignerApiConfigurator(IRestServiceSettings serviceSettings)
        {
            this.serviceSettings = serviceSettings;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            hc.BaseAddress = new Uri(serviceSettings.Endpoint);
            hc.Timeout = TimeSpan.FromMinutes(2);
            hc.DefaultRequestHeaders.Add("User-Agent", serviceSettings.UserAgent);
        }
    }
}
