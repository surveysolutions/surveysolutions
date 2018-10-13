using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.ExportProcessHandlers;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormatAsync(ExportSettings exportSettings, IProgress<int> progress, CancellationToken cancellationToken);

        Task GenerateDescriptionFileAsync(TenantInfo tenant, QuestionnaireId questionnaireId, string directoryPath, string tabDataFileExtension);
    }
}
