using System.Web.Routing;

namespace Designer.Web.NavigationRoutes
{
    public class NavigationRouteFilter : INavigationRouteFilter
    {
        public bool ShouldRemove(Route route)
        {
            return true;
        }
    }
}
