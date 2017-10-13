using System.IO;
using WB.Core.BoundedContexts.Headquarters.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IMapRepository
    {
        string StoreData(Stream preloadedDataFile, string fileName);
        MapFileDescription[] GetMapsMetaInformation(string id);
        string[] UnzipAndGetFileList(string id);
        void SaveOrUpdateMap(string map);
        void DeleteTempData(string id);

        void DeleteMap(string map);
    }
}
