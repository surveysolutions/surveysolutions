using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;

namespace WB.Enumerator.Native.WebInterview.Pipeline
{
    public interface IPipelineModule
    {
        Task OnConnected(IHub hub);
        Task OnDisconnected(IHub hub, bool stopCalled);
        Task OnReconnected(IHub hub);
    }
}
