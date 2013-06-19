using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.EventHandlers
{
    using System;
    using Main.Core.Events.Sync;
    using Ncqrs.Eventing.ServiceModel.Bus;
    using Main.Core.Documents;
    using Main.DenormalizerStorage;

    public class ClientDeviceDenormalizer : IEventHandler<NewClientDeviceCreated>
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
    }
}
