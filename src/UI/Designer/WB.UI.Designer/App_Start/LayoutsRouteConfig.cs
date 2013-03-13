using System.Web.Routing;
using WB.UI.Designer.Controllers;
using WB.UI.Designer.Models;
using WB.UI.Designer.NavigationRoutes;
using WB.UI.Designer.RouteFilters;

namespace WB.UI.Designer
{
    public class LayoutsRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // this enables menu suppression for routes with a FilterToken of "admin" set
            NavigationRouteFilters.Filters.Add(new AdministrationRouteFilter());

            routes.MapNavigationRoute<QuestionnaireController>("My Questionnaires", c => c.Index(null, null, null, null));
            routes.MapNavigationRoute<QuestionnaireController>("Public Questionnaires", c => c.Public(null, null, null, null), string.Empty,
                                                               new NavigationRouteOptions() {HasBreakAfter = true});

            routes.MapNavigationRoute<AccountController>("My Account", c => c.Login(string.Empty), string.Empty,
                                                         new NavigationRouteOptions() {HasBreakAfter = true})
                  .AddChildRoute<AccountController>("Manage", c => c.ExternalManage())
                  .AddChildRoute<AccountController>("Logout", c => c.LogOff());

            routes.MapNavigationRoute<AdministrationController>("Manage users", c => c.Index(null, null, null, null), string.Empty,
                                                                new NavigationRouteOptions()
                                                                    {
                                                                        FilterToken = UserHelper.ADMINROLENAME,
                                                                        HasBreakAfter = true
                                                                    });
        }
    }
}
