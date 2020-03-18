using WB.UI.Shared.Web.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class VirtualPathService : IVirtualPathService
    {
        public string GetAbsolutePath(string relativePath) => relativePath.TrimStart('~');
    }
}
