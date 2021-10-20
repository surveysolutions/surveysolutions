using System;
using Main.Core.Documents;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IImportExportQuestionnaireService
    {
        string Export(QuestionnaireDocument questionnaireDocument);
        QuestionnaireDocument Import(string json);
    }
}