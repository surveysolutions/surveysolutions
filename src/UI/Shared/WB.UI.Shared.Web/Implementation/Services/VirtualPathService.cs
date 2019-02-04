using System.Web;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Shared.Web.Implementation.Services
{
    public class VirtualPathService : IVirtualPathService
    {
        public string GetAbsolutePath(string relativePath) => VirtualPathUtility.ToAbsolute(relativePath);
    }
}
