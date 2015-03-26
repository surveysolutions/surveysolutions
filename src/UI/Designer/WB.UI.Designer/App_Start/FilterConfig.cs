using WB.UI.Designer.Filters;

namespace WB.UI.Designer.App_Start
{
    using System.Web.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //errors are handled in global.asax Application_Error
            //filters.Add(new HandleErrorAttribute());

            filters.Add(new MaintenanceFilter());
            filters.Add(new ShowNotesToUserFilter());
        }
    }
}