using System;
using Main.Core.Documents;
using WB.UI.Designer.Code.ImportExport.Models;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IImportExportQuestionnaireMapper
    {
        Questionnaire Map(QuestionnaireDocument questionnaireDocument);
        QuestionnaireDocument Map(Questionnaire questionnaire);
    }
}