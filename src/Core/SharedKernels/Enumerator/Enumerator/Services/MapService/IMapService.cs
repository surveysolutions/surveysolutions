using System.Collections.Generic;
using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public interface IMapService
    {
        List<MapDescription> GetAvailableMaps();
        bool DoesMapExist(string mapName);
        void SaveMap(string mapName, byte[] content);
        MapDescription PrepareAndGetDefaultMap();
        Stream GetTempMapSaveStream(string mapName);
        void MoveTempMapToPermanent(string mapName);

        void RemoveMap(string mapName);

        List<ShapefileDescription> GetAvailableShapefiles();
    }
}
