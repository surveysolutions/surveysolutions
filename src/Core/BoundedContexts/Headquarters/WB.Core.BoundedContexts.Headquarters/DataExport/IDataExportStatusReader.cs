using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public class DataExportArchive
    {
        public Stream Data { get; set; }
        public string Redirect { get; set; }
        public string FileName { get; set; }
    }

    public interface IDataExportStatusReader
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        Task<DataExportArchive> GetDataArchive(
            QuestionnaireIdentity questionnaireIdentity, DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null, Guid? translationId = null, bool? includeMeta = null);

        Task<ExportDataAvailabilityView> GetDataAvailabilityAsync(QuestionnaireIdentity questionnaireIdentity);

        Task<DataExportProcessView> GetProcessStatus(long id);
        Task<List<DataExportProcessView>> GetProcessStatuses(long[] id);

        Task<bool> WasExportFileRecreated(long id);
    }
}
