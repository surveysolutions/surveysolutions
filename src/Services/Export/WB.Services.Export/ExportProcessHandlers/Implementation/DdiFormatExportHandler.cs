using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Ddi;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Models;
using WB.Services.Export.Services.Processing;

namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class DdiFormatExportHandler : AbstractDataExportHandler
    {
        private readonly IDdiMetadataFactory ddiMetadataFactory;
        private readonly IExportFileNameService exportFileNameService;

        public DdiFormatExportHandler(IFileSystemAccessor fileSystemAccessor,
            IFileBasedExportedDataAccessor fileBasedExportedDataAccessor, 
            IOptions<ExportServiceSettings> interviewDataExportSettings, 
            IDataExportProcessesService dataExportProcessesService, 
            IDataExportFileAccessor dataExportFileAccessor,
            IDdiMetadataFactory ddiMetadataFactory,
            IExportFileNameService exportFileNameService) 
            : base(fileSystemAccessor, fileBasedExportedDataAccessor, interviewDataExportSettings, dataExportProcessesService, dataExportFileAccessor)
        {
            this.ddiMetadataFactory = ddiMetadataFactory;
            this.exportFileNameService = exportFileNameService;
        }

        protected override DataExportFormat Format => DataExportFormat.DDI;

        protected override async Task ExportDataIntoDirectory(ExportSettings settings, ExportProgress progress, CancellationToken cancellationToken)
        {
            await this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolderAsync(settings.Tenant, settings.QuestionnaireId, this.ExportTempDirectoryPath);
        }
    }
}
