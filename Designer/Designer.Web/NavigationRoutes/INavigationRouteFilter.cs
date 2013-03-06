using System.Web.Routing;

namespace Designer.Web.NavigationRoutes
{
    public interface INavigationRouteFilter
    {
        bool  ShouldRemove(Route navigationRoutes);
    }
}
