using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public interface IMapPropertiesProvider
    {
        Task<MapProperties> GetMapPropertiesFromFileAsync(string pathToFile);
        bool IsMapEngineOperational();
    }
}