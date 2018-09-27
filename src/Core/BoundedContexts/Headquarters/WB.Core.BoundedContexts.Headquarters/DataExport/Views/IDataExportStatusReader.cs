using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public interface IDataExportStatusReader
    {
        Task<DataExportStatusView> GetDataExportStatusForQuestionnaireAsync(string baseUrl,
            string apiKey,
            QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
