using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.CsvExport
{
    public interface ITabularFormatExportService
    {
        Task ExportInterviewsInTabularFormat(TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            string basePath,
            DateTime? fromDate,
            DateTime? toDate,
        IProgress<int> progress,
            CancellationToken cancellationToken);

        void GenerateDescriptionFile(TenantInfo tenant, QuestionnaireId questionnaireId, string directoryPath, string tabDataFileExtension);
    }
}
