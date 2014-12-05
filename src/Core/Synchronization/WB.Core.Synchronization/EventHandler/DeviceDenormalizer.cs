using Main.Core.Events.Synchronization;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization.Documents;

namespace WB.Core.Synchronization.EventHandler
{
    public class DeviceDenormalizer : IEventHandler<NewDeviceRegistered>,
                                      IEventHandler<UpdateRegisteredDevice>
    {
        private readonly IReadSideRepositoryWriter<SyncDeviceRegisterDocument> devices;

        public DeviceDenormalizer(IReadSideRepositoryWriter<SyncDeviceRegisterDocument> registerDevices)
        {
            this.devices = registerDevices;
        }


        public void Handle(IPublishedEvent<NewDeviceRegistered> evnt)
        {
            this.devices.Store(
                new SyncDeviceRegisterDocument
                {
                    CreationDate = evnt.Payload.RegisteredDate,
                    ModificationDate = evnt.Payload.RegisteredDate,
                    SecretKey = evnt.Payload.SecretKey,
                    Description = evnt.Payload.Description,
                    TabletId = evnt.Payload.IdForRegistration,
                    PublicKey = evnt.EventSourceId,
                    Registrator = evnt.Payload.Registrator
                },
                evnt.Payload.IdForRegistration);
        }

        public void Handle(IPublishedEvent<UpdateRegisteredDevice> evnt)
        {
            var device = this.devices.GetById(evnt.Payload.DeviceId);

            if (device == null)
            {
                return;
            }
            device.ModificationDate = evnt.EventTimeStamp;
            device.SecretKey = evnt.Payload.SecretKey;
            device.Registrator = evnt.Payload.Registrator;
            device.Description = evnt.Payload.Description;

            this.devices.Store(device, evnt.Payload.DeviceId);
        }
    }
}