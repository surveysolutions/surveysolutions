using System.Threading.Tasks;

namespace WB.UI.Headquarters.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewName, object model, string webRoot = null);
    }
}
