using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Device
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    public class DeviceViewFactory : IViewFactory<DeviceViewInputModel, DeviceView>
    {
        private readonly IQueryableReadSideRepositoryReader<SyncDeviceRegisterDocument> devices;

        public DeviceViewFactory(IQueryableReadSideRepositoryReader<SyncDeviceRegisterDocument> devices)
        {
            this.devices = devices;
        }

        public DeviceView Load(DeviceViewInputModel input)
        {
            return this.devices.Query(queryableDevices =>
            {
                int count = queryableDevices
                                .Where(d => d.Registrator != Guid.Empty).Count(d => d.Registrator == input.RegistratorId);

                if (count == 0)
                {
                    return new DeviceView(0, 0, 0, new List<RegisterData>(), string.Empty);
                }

                IQueryable<SyncDeviceRegisterDocument> query = queryableDevices.Where(d => d.Registrator != Guid.Empty).Where(d => d.Registrator == input.RegistratorId);
                var page = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
                var items = page.ToList();
                var devices = items.Select(item => new RegisterData() { Description = item.Description, RegisterDate = item.CreationDate, RegistrationId = item.PublicKey, Registrator = item.Registrator, SecretKey = item.SecretKey }).ToList();
                return new DeviceView(input.Page, input.PageSize, count, devices, input.Order);
            });
        }
    }
}
