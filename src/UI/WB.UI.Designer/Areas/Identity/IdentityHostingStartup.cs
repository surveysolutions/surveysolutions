using Microsoft.AspNetCore.Hosting;
using WB.UI.Designer.Areas.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace WB.UI.Designer.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}
