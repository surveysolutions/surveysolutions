using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SignalrDiagnosticHub : Hub
    {
        public Task Ping()
        {
            HttpTransportType transportType = Context.Features.Get<IHttpTransportFeature>().TransportType;
            return Clients.Caller.SendAsync("Pong", transportType.ToString());
        }
    }
}
