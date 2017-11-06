using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IMapRepository
    {
        string StoreData(Stream preloadedDataFile, string fileName);
        MapFileDescription[] GetMapsMetaInformation(string id);
        string[] UnzipAndGetFileList(string id);

        Task SaveOrUpdateMapAsync(string map);
        void DeleteData(string id);

        void DeleteMap(string map);

        void DeleteMapUser(string map, string user);

        string[][] GetAllMapUsers();

        bool UpdateUserMaps(string mapName, string[] users);

        string[] GetAllMapsForUser(string userName);

        byte[] GetMapContent(string mapName);
    }
}
