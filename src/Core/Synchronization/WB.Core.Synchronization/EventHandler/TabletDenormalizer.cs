using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
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
            return new TabletDocument
                   {
                       AndroidId = evnt.Payload.AndroidId,
                       DeviceId = evnt.EventSourceId,
                       RegistrationDate = evnt.EventTimeStamp,
                       Users = new List<Guid>()
                   };
        }
    }
}