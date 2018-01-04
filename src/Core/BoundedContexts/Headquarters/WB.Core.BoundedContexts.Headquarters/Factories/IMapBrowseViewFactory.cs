using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IMapBrowseViewFactory
    {
        MapsView Load(MapsInputModel input);

        MapUsersView Load(MapUsersInputModel input);

        UserMapsView Load(UserMapsInputModel input);
    }
}
