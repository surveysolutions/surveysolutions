using System;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    internal class DeviceExceptionRepository : IDeviceExceptionRepository
    {
        public void Add(DeviceException exception)
        {
            using (var dataContext = new HQPlainStorageDbContext())
            {
                dataContext.DeviceException.Add(exception);
                dataContext.SaveChanges();
            }
        }
    }
}