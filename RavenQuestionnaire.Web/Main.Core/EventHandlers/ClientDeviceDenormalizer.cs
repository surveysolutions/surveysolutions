using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.EventHandlers
{
    using System;
    using Main.Core.Events.Sync;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Main.Core.Documents;
    using Main.DenormalizerStorage;

    public class ClientDeviceDenormalizer : IEventHandler<NewClientDeviceCreated>, IEventHandler<ClientDeviceLastSyncItemUpdated>
    {
        private readonly IReadSideRepositoryWriter<ClientDeviceDocument> devices;

        public ClientDeviceDenormalizer(IReadSideRepositoryWriter<ClientDeviceDocument> devices)
        {
            this.devices = devices;
        }

        public void Handle(IPublishedEvent<NewClientDeviceCreated> evnt)
        {

            var doc = new ClientDeviceDocument()
                {
                    CreatedDate = evnt.Payload.CreationDate,
                    ModificationDate = evnt.Payload.CreationDate,
                    ClientInstanceKey = evnt.Payload.ClientInstanceKey,
                    Id = evnt.Payload.Id
                };

            this.devices.Store( doc, doc.Id);
        }

        public void Handle(IPublishedEvent<ClientDeviceLastSyncItemUpdated> evnt)
        {
            var item = this.devices.GetById(evnt.EventSourceId);
            item.LastSyncItemIdentifier = evnt.Payload.LastSyncItemSequence;
        }
    }
}
