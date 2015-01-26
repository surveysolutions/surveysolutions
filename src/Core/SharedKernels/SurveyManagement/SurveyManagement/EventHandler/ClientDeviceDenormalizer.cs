using System;
using Main.Core.Documents;
using Main.Core.Events.Sync;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class ClientDeviceDenormalizer : BaseDenormalizer, IEventHandler<NewClientDeviceCreated>,
                                            IEventHandler<ClientDeviceLastSyncItemUpdated>
    {
        private readonly IReadSideKeyValueStorage<ClientDeviceDocument> devices;

        public ClientDeviceDenormalizer(IReadSideKeyValueStorage<ClientDeviceDocument> devices)
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
                    DeviceId = evnt.Payload.DeviceId,
                    SupervisorKey = evnt.Payload.SupervisorKey
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

        public override object[] Writers
        {
            get { return new[] { devices }; }
        }
    }
}
