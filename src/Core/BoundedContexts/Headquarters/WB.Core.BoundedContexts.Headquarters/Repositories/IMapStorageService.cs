using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Headquarters.Repositories
{
    public interface IMapStorageService
    {
        Task SaveOrUpdateMapAsync(ExtractedFile map);
        void DeleteMap(string map);
        byte[] GetMapContent(string mapName);
        
        void DeleteMapUserLink(string map, string user);
        ReportView GetAllMapUsersReportView();
        void UpdateUserMaps(string mapName, string[] users);
        string[] GetAllMapsForInterviewer(string userName);
        string[] GetAllMapsForSupervisor(Guid supervisorId);
    }
}
