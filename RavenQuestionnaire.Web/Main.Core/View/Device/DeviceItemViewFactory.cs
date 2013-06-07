// -----------------------------------------------------------------------
// <copyright file="DeviceItemViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Main.Core.View.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
using Main.DenormalizerStorage;
using Main.Core.Documents;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceItemViewFactory  : IViewFactory<DeviceItemViewInputModel, DeviceItemView>
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
        public DeviceItemViewFactory(IDenormalizerStorage<SyncDeviceRegisterDocument> devices)
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
        public DeviceItemView Load(DeviceItemViewInputModel input)
        {
            var device = this.devices.GetById(input.DeviceId);

            if (device == null)
            {
                return new DeviceItemView();
            }

            return new DeviceItemView(device);
        }

        #endregion
    }
}
