using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IMapStorageService
    {
        Task<MapBrowseItem> SaveOrUpdateMapAsync(ExtractedFile map);
        Task<MapBrowseItem> DeleteMap(string map);
        Task DeleteAllMaps();
        Task<byte[]> GetMapContentAsync(string mapName);

        MapBrowseItem DeleteMapUserLink(string map, string user);
        ReportView GetAllMapUsersReportView();
        void UpdateUserMaps(string mapName, string[] users);
        string[] GetAllMapsForInterviewer(string userName);
        string[] GetAllMapsForSupervisor(Guid supervisorId);
        MapBrowseItem GetMapById(string id);
        MapBrowseItem AddUserToMap(string id, string userName);
    }
}
