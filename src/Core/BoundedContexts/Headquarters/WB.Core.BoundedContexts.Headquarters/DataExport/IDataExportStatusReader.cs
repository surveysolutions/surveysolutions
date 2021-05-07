#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.ServicesIntegration.Export;
using InterviewStatus = WB.Core.SharedKernels.DataCollection.ValueObjects.Interview.InterviewStatus;
using QuestionnaireIdentity = WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IDataExportStatusReader
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        Task<DataExportArchive?> GetDataArchive(long jobId);
        
        Task<ExportDataAvailabilityView?> GetDataAvailabilityAsync(QuestionnaireIdentity questionnaireIdentity);

        Task<DataExportProcessView?> GetProcessStatus(long id);
        Task<List<DataExportProcessView>> GetProcessStatuses(long[] id);

        Task<bool> WasExportFileRecreated(long id);
    }
}
