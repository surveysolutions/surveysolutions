// -----------------------------------------------------------------------
// <copyright file="DeviceViewFactory.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.View;
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
        private readonly IQueryableDenormalizerStorage<SyncDeviceRegisterDocument> devices;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewFactory"/> class.
        /// </summary>
        /// <param name="devices">
        /// The devices.
        /// </param>
        public DeviceViewFactory(IQueryableDenormalizerStorage<SyncDeviceRegisterDocument> devices)
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
            int count = this.devices.Query(_ => _
                .Where(d => d.Registrator != Guid.Empty)
                .Where(d => d.Registrator == input.RegistratorId).Count());

            if (count == 0)
            {
                return new DeviceView(0, 0, 0, new List<RegisterData>(), string.Empty);
            }

            List<SyncDeviceRegisterDocument> items = this.devices.Query(queryable =>
            {
                IQueryable<SyncDeviceRegisterDocument> query = queryable.Where(d => d.Registrator != Guid.Empty).Where(d => d.Registrator == input.RegistratorId);
                var page = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
                return page.ToList();
            });
            var devices = items.Select(item => new RegisterData() { Description = item.Description, RegisterDate = item.CreationDate, RegistrationId = item.PublicKey, Registrator = item.Registrator, SecretKey = item.SecretKey }).ToList();
            return new DeviceView(input.Page, input.PageSize, count, devices, input.Order);
        }

        #endregion
    }
}
