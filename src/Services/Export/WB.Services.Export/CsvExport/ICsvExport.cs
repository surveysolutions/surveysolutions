using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.CsvExport
{
    public interface ICsvExport
    {
        Task ExportInterviewsInTabularFormat(TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            string basePath,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
