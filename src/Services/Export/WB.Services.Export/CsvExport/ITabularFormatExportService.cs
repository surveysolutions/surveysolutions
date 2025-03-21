using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormatAsync(ExportSettings exportSettings, string tempPath, ExportProgress progress, CancellationToken cancellationToken);
        Task GenerateDescriptionFileAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string directoryPath, string tabDataFileExtension, CancellationToken cancellationToken);
        Task GenerateShortDescriptionFileAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string directoryPath, string tabDataFileExtension, DataExportFormat dataExportFormat, bool? paradataReduced, CancellationToken cancellationToken);
        Task GenerateInformationFileAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string directoryPath, CancellationToken cancellationToken);
    }
}
