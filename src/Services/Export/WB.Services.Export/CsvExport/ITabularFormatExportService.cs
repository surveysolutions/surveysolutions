using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;
using WB.ServicesIntegration.Export;

namespace WB.Services.Export.CsvExport
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormatAsync(ExportSettings exportSettings, string tempPath, ExportProgress progress, CancellationToken cancellationToken);

        Task GenerateDescriptionFileAsync(TenantInfo tenant, QuestionnaireIdentity questionnaireId, string directoryPath, string tabDataFileExtension, CancellationToken cancellationToken);
    }
}
