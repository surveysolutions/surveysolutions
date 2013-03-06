using Designer.Web.Controllers;
using Designer.Web.Models;
using Designer.Web.NavigationRoutes;
using Designer.Web.RouteFilters;
using System.Web.Routing;

namespace Designer.Web
{
    public class LayoutsRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // this enables menu suppression for routes with a FilterToken of "admin" set
            NavigationRouteFilters.Filters.Add(new AdministrationRouteFilter());

            routes.MapNavigationRoute<QuestionnairesController>("My Questionnaires", c => c.Index());
            routes.MapNavigationRoute<QuestionnairesController>("Public Questionnaires", c => c.Public());

            routes.MapNavigationRoute<AccountController>("My Account", c => c.Login(string.Empty))
                  .AddChildRoute<AccountController>("Manage", c => c.ExternalManage())
                  .AddChildRoute<AccountController>("Logout", c => c.LogOff());

            routes.MapNavigationRoute<AdministrationController>("Manage users", c => c.Index(), string.Empty,
                                                                new NavigationRouteOptions()
                                                                    {
                                                                        FilterToken = UserHelper.ADMINROLENAME,
                                                                        HasBreakAfter = true
                                                                    });
        }
    }
}
