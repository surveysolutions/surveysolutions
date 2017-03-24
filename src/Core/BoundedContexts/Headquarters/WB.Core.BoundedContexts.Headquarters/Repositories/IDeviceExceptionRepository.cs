using System;
using WB.Core.BoundedContexts.Headquarters.Views.Device;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IDeviceExceptionRepository
    {
        void Add(DeviceException exception);
    }
}