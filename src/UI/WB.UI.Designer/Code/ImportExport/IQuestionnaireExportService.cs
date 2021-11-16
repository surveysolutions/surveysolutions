using System;
using System.IO;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireExportService
    {
        Stream? GetBackupQuestionnaire(Guid id, out string questionnaireFileName);
    }
}