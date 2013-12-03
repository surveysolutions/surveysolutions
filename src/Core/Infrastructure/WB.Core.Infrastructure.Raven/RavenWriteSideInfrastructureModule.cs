using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Raven.Client.Document;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenWriteSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly int pageSize;
        private IStreamableEventStore singleEventStore;

        public RavenWriteSideInfrastructureModule(RavenConnectionSettings settings, int pageSize = 50)
            : base(settings)
        {
            this.pageSize = pageSize;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            NcqrsEnvironment.SetGetter<IStreamableEventStore>(this.GetEventStore);
            NcqrsEnvironment.SetGetter<IEventStore>(this.GetEventStore);
            this.Kernel.Bind<IStreamableEventStore>().ToMethod(_ => this.GetEventStore());
            this.Kernel.Bind<IEventStore>().ToMethod(_ => this.GetEventStore());
        }

        private IStreamableEventStore GetEventStore()
        {
            return this.singleEventStore ?? (this.singleEventStore =
                new RavenDBEventStore(
                    this.Kernel.Get<DocumentStoreProvider>().CreateSeparateInstanceForEventStore(),
                    this.pageSize));
        }
    }
}