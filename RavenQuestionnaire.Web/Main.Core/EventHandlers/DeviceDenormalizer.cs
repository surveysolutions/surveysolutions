// -----------------------------------------------------------------------
// <copyright file="DeviceDenormalizer.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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
    public class DeviceDenormalizer : IEventHandler<NewDeviceRegistered>
    {
        #region Fields

        /// <summary>
        /// Devices field
        /// </summary>
        private readonly IDenormalizerStorage<SyncDeviceRegisterDocument> devices;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceDenormalizer"/> class.
        /// </summary>
        /// <param name="registerDevices">
        /// The register devices.
        /// </param>
        public DeviceDenormalizer(IDenormalizerStorage<SyncDeviceRegisterDocument> registerDevices)
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
                    CreationDate = DateTime.UtcNow,
                    SecretKey = evnt.Payload.SecretKey,
                    Description = evnt.Payload.Description,
                    TabletId = evnt.Payload.TabletId
                },
                evnt.Payload.PublicKey);
        }

        #endregion

    }
}
