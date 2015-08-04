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

        public TabletDocument Update(TabletDocument currentState, IPublishedEvent<TabletRegistered> evnt)
        {
            return new TabletDocument {
                       Id = evnt.EventSourceId.FormatGuid(),
                       AndroidId = evnt.Payload.AndroidId,
                       DeviceId = evnt.EventSourceId,
                       RegistrationDate = evnt.EventTimeStamp
                   };
        }
    }
}