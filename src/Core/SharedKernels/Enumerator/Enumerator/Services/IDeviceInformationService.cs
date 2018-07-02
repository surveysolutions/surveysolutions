using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IDeviceInformationService : IDisposable
    {
        Task<DeviceInfo> GetDeviceInfoAsync();
    }
}
