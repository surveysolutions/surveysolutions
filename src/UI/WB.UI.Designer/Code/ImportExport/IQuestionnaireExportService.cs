using System;
using System.IO;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireExportService
    {
        Stream? GetBackupQuestionnaire(QuestionnaireRevision id, out string questionnaireFileName);
    }
}