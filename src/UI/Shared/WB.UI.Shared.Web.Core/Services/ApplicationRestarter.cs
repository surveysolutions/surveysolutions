using Microsoft.Extensions.Hosting;
using WB.Core.Infrastructure.Modularity;

namespace WB.UI.Shared.Web.Services
{
    public class ApplicationRestarter : IApplicationRestarter
    {
        private readonly IHostApplicationLifetime applicationLifetime;

        public ApplicationRestarter(IHostApplicationLifetime applicationLifetime)
        {
            this.applicationLifetime = applicationLifetime;
        }

        public void Restart()
        {
            applicationLifetime.StopApplication();
        }
    }
}
