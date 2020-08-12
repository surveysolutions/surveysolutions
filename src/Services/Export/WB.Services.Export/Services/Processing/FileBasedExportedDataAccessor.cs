using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Services.Processing
{
    class FileBasedExportedDataAccessor : IFileBasedExportedDataAccessor
    {
        private readonly IExportFileNameService exportFileNameService;
        private readonly IOptions<ExportServiceSettings> interviewDataExportSettings;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public FileBasedExportedDataAccessor(
            IExportFileNameService exportFileNameService, 
            IFileSystemAccessor fileSystemAccessor, 
            IOptions<ExportServiceSettings> interviewDataExportSettings)
        {
            this.exportFileNameService = exportFileNameService;
            this.fileSystemAccessor = fileSystemAccessor;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }
        
        public async Task<string> GetArchiveFilePathForExportedDataAsync(ExportSettings exportSettings)
        {
            var exportDirectory = GetExportDirectory(exportSettings.Tenant);
            CreateIfNeeded(exportDirectory);

            var fileName = await this.exportFileNameService.GetFileNameForExportArchiveAsync(exportSettings);

            return Path.Combine(exportDirectory, fileName);
        }

        private void CreateIfNeeded(string path)
        {
            if (!fileSystemAccessor.IsDirectoryExists(path))
                fileSystemAccessor.CreateDirectory(path);
        }

        public string GetExportDirectory(TenantInfo tenant)
        {
            return this.fileSystemAccessor.CombinePath(
                interviewDataExportSettings.Value.DirectoryPath,
                tenant.Id.ToString());
        }
    }
}
