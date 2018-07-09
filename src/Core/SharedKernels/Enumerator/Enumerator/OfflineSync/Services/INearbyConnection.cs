using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface INearbyConnection
    {
        Task StartDiscovery(string serviceName);
        Task<string> StartAdvertising(string serviceName, string name);
        Task RequestConnection(string name, string endpoint);
        Task AcceptConnection(string endpoint);
        Task RejectConnection(string endpoint);
        Task SendPayloadAsync(string to, IPayload payload);

        IObservable<INearbyEvent> Events { get; }
        ObservableCollection<RemoteEndpoint> RemoteEndpoints { get; }
        Task StopDiscovery();
        Task StopAdvertising();
    }
}
