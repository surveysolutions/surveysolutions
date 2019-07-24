using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public interface IMapService
    {
        Task<MapProperties> GetMapPropertiesFromFileAsync(string pathToFile);
        bool IsEngineEnabled();
    }
}