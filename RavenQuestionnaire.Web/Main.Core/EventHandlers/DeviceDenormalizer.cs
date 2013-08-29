using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.EventHandlers
{
    using System;
    using Main.Core.Documents;
    using Main.Core.Events.Synchronization;
    using Main.DenormalizerStorage;
    using Ncqrs.Eventing.ServiceModel.Bus;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceDenormalizer : IEventHandler<NewDeviceRegistered>,
                                      IEventHandler<UpdateRegisteredDevice>
    {
        #region Fields

        /// <summary>
        /// Devices field
        /// </summary>
        private readonly IReadSideRepositoryWriter<SyncDeviceRegisterDocument> devices;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDenormalizer"/> class.
        /// </summary>
        /// <param name="registerDevices">
        /// The register devices.
        /// </param>
        public DeviceDenormalizer(IReadSideRepositoryWriter<SyncDeviceRegisterDocument> registerDevices)
        {
            this.devices = registerDevices;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add new device 
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
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


        #endregion


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