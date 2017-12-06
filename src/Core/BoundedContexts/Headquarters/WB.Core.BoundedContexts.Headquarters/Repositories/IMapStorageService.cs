using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IMapStorageService
    {
        string[] UnzipAndGetFileList(byte[] fileStreamByteArray, out string tempStore);

        Task SaveOrUpdateMapAsync(string map);
        void DeleteTemporaryData(string id);
        void DeleteMap(string map);
        byte[] GetMapContent(string mapName);

        
        void DeleteMapUserLink(string map, string user);
        ReportView GetAllMapUsersReportView();
        void UpdateUserMaps(string mapName, string[] users);
        string[] GetAllMapsForUser(string userName);
    }
}
