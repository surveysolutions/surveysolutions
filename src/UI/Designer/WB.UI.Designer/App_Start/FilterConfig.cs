using System.Web;
using System.Web.Mvc;

namespace WB.UI.Designer
{
    using WB.UI.Designer.Filters;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequiresReadLayerFilter());
        }
    }
}