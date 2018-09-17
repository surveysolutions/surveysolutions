using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Interview.Exporters
{
    public interface IExportQuestionService
    {
        string[] GetExportedQuestion(InterviewEntity question, ExportedQuestionHeaderItem header);
        string[] GetExportedVariable(object variable, ExportedVariableHeaderItem header, bool isDisabled);
    }
}