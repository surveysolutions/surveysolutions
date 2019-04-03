using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

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
