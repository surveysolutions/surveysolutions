using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;
using WB.Core.Synchronization.Events.Sync;

namespace WB.Core.Synchronization.EventHandler
{
    public class TabletDenormalizer : AbstractFunctionalEventHandler<TabletDocument, IReadSideRepositoryWriter<TabletDocument>>,
         IUpdateHandler<TabletDocument, TabletRegistered>
    {

        public TabletDenormalizer(IReadSideRepositoryWriter<TabletDocument> tabletDocumentsStroraWriter)
            : base(tabletDocumentsStroraWriter)
        {
        }

        public TabletDocument Update(TabletDocument state, IPublishedEvent<TabletRegistered> @event)
        {
            return new TabletDocument {
                       Id = @event.EventSourceId.FormatGuid(),
                       AndroidId = @event.Payload.AndroidId,
                       DeviceId = @event.EventSourceId,
                       RegistrationDate = @event.EventTimeStamp
                   };
        }
    }
}