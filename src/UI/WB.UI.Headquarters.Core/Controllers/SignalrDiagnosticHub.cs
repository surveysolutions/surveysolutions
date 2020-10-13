using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SignalrDiagnosticHub : Hub
    {
        public Task Ping()
        {
            return Clients.All.SendAsync("Pong");
        }
    }
}
