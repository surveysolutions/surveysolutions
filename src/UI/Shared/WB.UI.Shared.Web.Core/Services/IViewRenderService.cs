using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace WB.UI.Shared.Web.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model, string webRoot = null, RouteData routeData = null);
    }
}
