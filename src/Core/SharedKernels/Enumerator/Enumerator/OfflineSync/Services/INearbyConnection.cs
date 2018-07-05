using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyConnection
    {
        Task StartDiscovery(string serviceName, Action<string, NearbyDiscoveredEndpointInfo> foundEndpoint, Action<string> lostEndpoint);
        Task<string> StartAdvertising(string serviceName, string name, NearbyConnectionLifeCycleCallback lifeCycleCallback);
        Task RequestConnection(string name, string endpoint, NearbyConnectionLifeCycleCallback lifeCycleCallback);
        Task AcceptConnection(string endpoint);
        Task RejectConnection(string endpoint);
        Task SendPayloadAsync(string to, IPayload payload);
    }
}
