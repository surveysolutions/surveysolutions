using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WB.UI.Shared.Web.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void AddHostedService<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, IHostedService, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddHostedService(c => (TImplementation)c.GetRequiredService<TService>());
        }
    }
}
