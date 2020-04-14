using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services.Processing;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.Jobs
{
    public interface IJobsStatusReporting
    {
        Task<DataExportProcessView> GetDataExportStatusAsync(long processId, TenantInfo tenant);
        Task<List<DataExportProcessView>> GetDataExportStatusesAsync(long[] processIds, TenantInfo tenant);

        Task<IEnumerable<DataExportProcessView>> GetDataExportStatusesAsync(DataExportFormat? exportType,
            InterviewStatus? interviewStatus, string questionnaireIdentity, DataExportJobStatus? exportStatus, bool? hasFile, int? limit, int? offset, TenantInfo tenant);

        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            TenantInfo tenant,
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
