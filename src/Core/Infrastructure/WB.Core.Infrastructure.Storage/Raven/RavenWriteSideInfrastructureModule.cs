using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using Raven.Client.Document;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenWriteSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly int pageSize;
        private IStreamableEventStore singleEventStore;
        private readonly bool useStreamingForAllEvents;
        private readonly FailoverBehavior failoverBehavior;

        public RavenWriteSideInfrastructureModule(RavenConnectionSettings settings, bool useStreamingForAllEvents = true, int pageSize = 50)
            : base(settings)
        {
            this.pageSize = pageSize;
            this.failoverBehavior = settings.FailoverBehavior;
            this.useStreamingForAllEvents = useStreamingForAllEvents;
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
                    this.pageSize, this.failoverBehavior, this.useStreamingForAllEvents));
        }
    }
}