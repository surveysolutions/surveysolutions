using System.Web;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Services
{
    public class AspNetAppPathResolver : IApplicationPathResolver
    {
        public string MapPath(string relativePath)
        {
            return HttpContext.Current.Server.MapPath(relativePath);
        }
    }
}
