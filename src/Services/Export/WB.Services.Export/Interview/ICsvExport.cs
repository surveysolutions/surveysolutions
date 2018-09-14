using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Tenant;

namespace WB.Services.Export.Interview
{
    public interface ICsvExport
    {
        Task ExportInterviewsInTabularFormat(string tenantBaseUrl,
            TenantId tenantId,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status,
            string basePath,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
