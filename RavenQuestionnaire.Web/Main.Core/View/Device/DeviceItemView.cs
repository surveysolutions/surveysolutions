namespace Main.Core.View.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Main.Core.Documents;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceItemView
    {
        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets RegisterDate.
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Gets or sets SecretKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// The ID of registrator who puts registration event to database
        /// </summary>
        public Guid Registrator { get; set; }

        /// <summary>
        /// The ID of device or another entity to be registered
        /// </summary>
        public Guid RegistrationId { get; set; }

        public DeviceItemView()
        {
            this.RegistrationId = Guid.Empty;
            this.Registrator = Guid.Empty;
        }

        public DeviceItemView(SyncDeviceRegisterDocument device)
        {
            this.Description = device.Description;
            this.RegisterDate = device.CreationDate;
            this.Registrator = device.Registrator;
            this.RegistrationId = device.TabletId;
            this.SecretKey = device.SecretKey;
        }
    }
}
