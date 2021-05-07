using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Ddi;
using WB.Services.Export.Services.Processing;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.ExportProcessHandlers.Implementation.Handlers
{
    internal class DdiFormatExportHandler : IExportHandler
    {
        private readonly IDdiMetadataFactory ddiMetadataFactory;

        public DdiFormatExportHandler(
            IDdiMetadataFactory ddiMetadataFactory) 
        {
            this.ddiMetadataFactory = ddiMetadataFactory;
        }

        public DataExportFormat Format => DataExportFormat.DDI;

        public async Task ExportDataAsync(ExportState state, CancellationToken cancellationToken)
        {
            var settings = state.Settings;

            await this.ddiMetadataFactory.CreateDDIMetadataFileForQuestionnaireInFolderAsync(
                settings.Tenant, settings.QuestionnaireId, state.ExportTempFolder);
        }
    }
}
