#nullable enable
using System.Collections.Generic;
using System.IO;

namespace WB.Core.SharedKernels.Enumerator.Services.MapService
{
    public interface IMapService
    {
        List<MapDescription> GetAvailableMaps(bool includeOnline = false);
        bool DoesMapExist(string mapName);
        MapDescription? PrepareAndGetDefaultMapOrNull();
        Stream GetTempMapSaveStream(string mapName);
        long GetTempMapOffset(string mapName);
        void SaveTempMapETag(string mapName, string? etag);
        string? GetTempMapETag(string mapName);
        void MoveTempMapToPermanent(string mapName);

        void RemoveMap(string mapName);

        List<ShapefileDescription> GetAvailableShapefiles();
    }
}
