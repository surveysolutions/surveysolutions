namespace WB.UI.Designer.App_Start
{
    using System.Web.Mvc;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //errors are handled id global.asax Application_Error
            //filters.Add(new HandleErrorAttribute());
        }
    }
}