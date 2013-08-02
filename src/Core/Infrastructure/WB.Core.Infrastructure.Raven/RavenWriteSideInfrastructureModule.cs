using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Raven.Client.Document;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenWriteSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly int pageSize;

        public RavenWriteSideInfrastructureModule(RavenConnectionSettings settings, int pageSize = 50)
            : base(settings)
        {
            this.pageSize = pageSize;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            var store = new RavenDBEventStore(this.Kernel.Get<DocumentStoreProvider>().CreateSeparateInstanceForEventStore(), this.pageSize);
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(store);
            NcqrsEnvironment.SetDefault<IEventStore>(store); // usage in framework 
            this.Kernel.Bind<IStreamableEventStore>().ToConstant(store);
        }
    }
}