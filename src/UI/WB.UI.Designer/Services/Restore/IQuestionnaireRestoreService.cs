using System;
using System.IO;

namespace WB.UI.Designer.Services.Restore
{
    public interface IQuestionnaireRestoreService
    {
        Guid RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, bool createNew);
    }
}
