using WB.Services.Export.Models;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    public interface IFileBasedExportedDataAccessor
    {
        string GetArchiveFilePathForExportedData(ExportSettings exportSettings);

        string GetExportDirectory(TenantInfo tenant);
    }
}
