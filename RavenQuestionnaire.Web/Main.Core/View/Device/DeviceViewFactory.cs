// -----------------------------------------------------------------------
// <copyright file="DeviceViewFactory.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.Device
{
    using System;
    using System.Collections.Generic;
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
            int count = this.devices.Query().Where(d => d.Registrator.Id == input.SupervisorId).Count();

            if (count == 0)
            {
                return new DeviceView(0, 0, 0, new List<SyncDeviceRegisterDocument>(), string.Empty);
            }

            IQueryable<SyncDeviceRegisterDocument> query = this.devices.Query().Where(d => d.Registrator.Id == input.SupervisorId);
            if (input.TabletId != Guid.Empty)
            {
                query = query.Where(t => t.TabletId == input.TabletId);
            }

            var page = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            var items = page.ToList();
            return new DeviceView(input.Page, input.PageSize, count, items, input.Order);
        }

        #endregion
    }
}
