using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using ActionContext = Microsoft.AspNetCore.Mvc.ActionContext;

namespace WB.UI.Designer.Code
{
    public static class GlobalHelper
    {
        public const string EmptyString = "--//--";

        public const int GridPageItemsCount = 50;

        public static string CurrentAction(this ViewContext viewContext)
        {
            return (string) viewContext.RouteData.Values["Action"];
        }

        public static string CurrentController(this ViewContext viewContext)
        {
            return (string) viewContext.RouteData.Values["Controller"];
        }
    }
}
