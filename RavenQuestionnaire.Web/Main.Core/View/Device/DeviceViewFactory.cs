// -----------------------------------------------------------------------
// <copyright file="DeviceViewFactory.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.Device
{
    using System.Linq;
    using Main.Core.Documents;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceViewFactory : IViewFactory<DeviceViewInputModel, DeviceView>
    {
        #region Field

        /// <summary>
        /// Devices field devices
        /// </summary>
        private readonly IDenormalizerStorage<SyncDeviceRegisterDocument> devices;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewFactory"/> class.
        /// </summary>
        /// <param name="devices">
        /// The devices.
        /// </param>
        public DeviceViewFactory(IDenormalizerStorage<SyncDeviceRegisterDocument> devices)
        {
            this.devices = devices;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find device from storage in memory
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// return DeviceView
        /// </returns>
        public DeviceView Load(DeviceViewInputModel input)
        {
            int count = devices.Count();
            if (count == 0) 
                return new DeviceView();
            var dev = devices.Query().Where(t =>t.TabletId == input.TabletId).FirstOrDefault();
            return new DeviceView(dev.Description, dev.CreationDate, dev.SecretKey, dev.TabletId);
        }

        #endregion
    }
}
