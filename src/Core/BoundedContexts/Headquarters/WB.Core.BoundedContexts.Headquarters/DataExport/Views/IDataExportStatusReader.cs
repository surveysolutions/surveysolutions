using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Views
{
    public interface IDataExportStatusReader
    {
        DataExportStatusView GetDataExportStatusForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, InterviewStatus? status = null);
    }
}