using Ninject;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.PlainStorage;
using WB.Core.Infrastructure.Storage.Raven.PlainStorage;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenPlainStorageInfrastructureModule : RavenInfrastructureModule
    {
        public RavenPlainStorageInfrastructureModule(RavenConnectionSettings settings)
            : base(settings) {}

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IRavenPlainStorageProvider>()
                .ToMethod(ctx => new RavenPlainStorageProvider(this.Kernel.Get<DocumentStoreProvider>().CreateInstanceForPlainStorage())).InSingletonScope();

            this.Bind(typeof (IPlainStorageAccessor<>)).To(typeof (RavenPlainStorageAccessor<>));
        }
    }
}