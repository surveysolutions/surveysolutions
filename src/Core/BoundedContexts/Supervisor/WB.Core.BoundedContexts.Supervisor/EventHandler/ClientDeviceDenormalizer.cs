using System;
using Main.Core.Documents;
using Main.Core.Events.Sync;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class ClientDeviceDenormalizer : IEventHandler<NewClientDeviceCreated>,
                                            IEventHandler<ClientDeviceLastSyncItemUpdated>,
                                            IEventHandler
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

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new Type[] {typeof (ClientDeviceDocument)}; }
        }
    }
}
