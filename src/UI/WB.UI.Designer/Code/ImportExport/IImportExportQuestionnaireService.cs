using System;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IImportExportQuestionnaireService
    {
        string Export(Guid questionnaireId);
    }
}