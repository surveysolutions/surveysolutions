using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.PlainStorage;
using WB.Core.Infrastructure.Raven.PlainStorage;

namespace WB.Core.Infrastructure.Raven
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
            this.Bind(typeof (IQueryablePlainStorageAccessor<>)).To(typeof (RavenQueryablePlainStorageAccessor<>));
        }
    }
}