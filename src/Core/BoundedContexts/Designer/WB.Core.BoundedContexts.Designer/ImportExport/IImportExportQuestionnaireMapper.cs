using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public interface IImportExportQuestionnaireMapper
    {
        Questionnaire Map(QuestionnaireDocument questionnaireDocument);
        QuestionnaireDocument Map(Questionnaire questionnaire);
    }
}