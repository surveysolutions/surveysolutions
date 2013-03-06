using System;
using Designer.Web.Models;
using System.Web;
using Designer.Web.NavigationRoutes;

namespace Designer.Web.RouteFilters
{
    public class AdministrationRouteFilter : INavigationRouteFilter
    {
        // an excercise for the reader would be to load the role name 
        // from your config file so this isn't compiled in, or add a constructor
        // that accepts a role name to use to make this a more generic filter
        private string AdministrationRole = UserHelper.ADMINROLENAME;

        public bool ShouldRemove(System.Web.Routing.Route navigationRoutes)
        {
            if (navigationRoutes.DataTokens.HasFilterToken())
            {
                var filterToken = navigationRoutes.DataTokens.FilterToken();
                var result = !HttpContext.Current.User.IsInRole(AdministrationRole) &&
                             (string.Compare(filterToken, AdministrationRole,
                                             StringComparison.InvariantCultureIgnoreCase) == 0);
                    
                return result;
            }

            return false;

        }
    }
}