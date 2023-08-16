using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using WB.Infrastructure.AspNetCore;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class DesignerApiConfigurator : IHttpClientConfigurator<IDesignerWebTesterApi>
    {
        private readonly string designerAddress;

        public DesignerApiConfigurator(IConfiguration configuration)
        {
            var designerAddress = configuration["DesignerAddress"];
            this.designerAddress = designerAddress != null ? designerAddress.TrimEnd('/') : String.Empty;
        }

        public void ConfigureHttpClient(HttpClient hc)
        {
            hc.MaxResponseContentBufferSize = 2_000_000_000;
            hc.BaseAddress = new Uri(designerAddress);
            hc.Timeout = TimeSpan.FromMinutes(5);
        }
    }
}
