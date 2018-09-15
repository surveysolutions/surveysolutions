using Autofac;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Infrastructure.Implementation;

namespace WB.Services.Export.Host.Infra
{
    public static class TenantApiExtensions
    {
        public static void AddTenantApi(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(TenantApi<>))
                .As(typeof(ITenantApi<>)).InstancePerLifetimeScope();
        }
    }
}
