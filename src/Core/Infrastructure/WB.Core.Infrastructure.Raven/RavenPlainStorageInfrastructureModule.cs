using Ninject;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.PlainStorage;
using WB.Core.Infrastructure.Raven.PlainStorage;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenPlainStorageInfrastructureModule : RavenInfrastructureModule
    {
        public RavenPlainStorageInfrastructureModule(RavenConnectionSettings settings)
            : base(settings)
        {
        }

        public override void Load()
        {
            this.Bind<IRavenPlainStorageProvider>()
                .ToMethod(ctx => new RavenPlainStorageProvider(this.Kernel.Get<DocumentStoreProvider>().CreateInstanceForPlainStorage()));
        }
    }
}