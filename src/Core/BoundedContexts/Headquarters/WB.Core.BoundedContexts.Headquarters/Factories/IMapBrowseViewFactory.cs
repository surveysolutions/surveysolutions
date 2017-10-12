using WB.Core.BoundedContexts.Headquarters.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Factories
{
    public interface IMapBrowseViewFactory
    {
        MapsView Load(MapsInputModel input);
    }
}
