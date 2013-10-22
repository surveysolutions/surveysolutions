using System;

namespace Main.Core.View.Device
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceItemViewInputModel
    {
        public DeviceItemViewInputModel(Guid deviceId)
        {
            this.DeviceId = deviceId;
        }


        /// <summary>
        /// Gets or sets RegistratorId.
        /// </summary>
        public Guid DeviceId { get; set; }
    }
}
