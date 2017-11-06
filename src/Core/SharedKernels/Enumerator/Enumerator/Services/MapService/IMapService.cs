using System.Collections.Generic;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public interface IMapService
    {
        List<MapDescription> GetAvailableMaps();
        bool DoesMapExist(string mapName);
        void SaveMap(string mapName, byte[] content);
    }
}