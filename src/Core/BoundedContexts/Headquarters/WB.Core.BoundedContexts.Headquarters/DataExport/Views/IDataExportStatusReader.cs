using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public class DataExportArchive
    {
        public Stream Data { get; set; }
        public Uri Redirect { get; set; }
        public string FileName { get; set; }
    }

    public interface IDataExportStatusReader
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(string baseUrl,
            string apiKey,
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);

        Task<DataExportArchive> GetDataArchive(
            string baseUrl, string apiKey,
            QuestionnaireIdentity questionnaireIdentity, DataExportFormat format,
            InterviewStatus? status = null, DateTime? from = null, DateTime? to = null);
    }
}
