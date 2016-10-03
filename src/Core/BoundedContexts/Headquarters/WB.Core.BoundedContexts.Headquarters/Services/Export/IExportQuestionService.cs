using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.Export
{
    public interface IExportQuestionService
    {
        string[] GetExportedQuestion(InterviewQuestion question, ExportedHeaderItem header);
    }
}
