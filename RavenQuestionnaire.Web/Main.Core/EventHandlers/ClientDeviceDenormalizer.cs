using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.EventHandlers
{
    using Main.Core.Events.Sync;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Main.Core.Documents;

    public class ClientDeviceDenormalizer : IEventHandler<NewClientDeviceCreated>, 
                                            IEventHandler<ClientDeviceLastSyncItemUpdated>
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
                    PublicKey = evnt.Payload.Id,
                    DeviceId = evnt .Payload.DeviceId
                };

            this.devices.Store(doc, doc.PublicKey);
        }

        public void Handle(IPublishedEvent<ClientDeviceLastSyncItemUpdated> evnt)
        {
            var item = this.devices.GetById(evnt.EventSourceId);
            item.LastSyncItemIdentifier = evnt.Payload.LastSyncItemSequence;
            item.ModificationDate = evnt.Payload.ChangeDate;

            this.devices.Store(item, item.PublicKey);
        }
    }
}
