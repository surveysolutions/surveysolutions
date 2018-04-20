using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Views;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public interface IDeviceInformationService : IDisposable
    {
        Task<DeviceInfo> GetDeviceInfoAsync();
        int TryGetApplicationVersionCode();
    }
}
