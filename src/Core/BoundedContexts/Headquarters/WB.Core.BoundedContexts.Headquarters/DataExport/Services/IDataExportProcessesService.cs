using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportDetails;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    public interface IDataExportProcessesService
    {
        Task AddDataExportAsync(DataExportProcessDetails details);

        DataExportProcessDetails[] GetRunningExportProcesses();

        void DeleteDataExport(string processId);
    }
}
