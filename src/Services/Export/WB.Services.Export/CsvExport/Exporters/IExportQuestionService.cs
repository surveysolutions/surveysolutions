using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Models;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.CsvExport.Exporters
{
    public interface IExportQuestionService
    {
        string[] GetExportedQuestion(InterviewEntity? question, ExportedQuestionHeaderItem header,
            GeographyExportFormat geographyExportFormat = GeographyExportFormat.Wkt);
        string[] GetExportedVariable(object? variable, ExportedVariableHeaderItem header, bool isDisabled);
    }
}
