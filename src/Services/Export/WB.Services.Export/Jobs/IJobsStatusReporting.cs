using System;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    public interface IJobsStatusReporting
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            string archiveFileName,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
