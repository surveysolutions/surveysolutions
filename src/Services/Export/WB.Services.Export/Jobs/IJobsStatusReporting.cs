﻿using System;
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
        Task<DataExportProcessView?> GetDataExportStatusAsync(long processId);
        Task<List<DataExportProcessView>> GetDataExportStatusesAsync(long[] processIds);

        Task<IEnumerable<DataExportProcessView>> GetDataExportStatusesAsync(DataExportFormat? exportType,
            InterviewStatus? interviewStatus, string? questionnaireIdentity, DataExportJobStatus? exportStatus, bool? hasFile, int? limit, int? offset);

        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireId questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
