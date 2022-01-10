using System;
using System.IO;
using WB.UI.Designer.Services.Restore;

namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireImportService
    {
        Guid RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, bool createNew);
    }
}