using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IFileBasedExportedDataAccessor
    {
        Task<string> GetArchiveFilePathForExportedDataAsync(ExportSettings exportSettings);

        string GetExportDirectory(TenantInfo tenant);
    }
}
