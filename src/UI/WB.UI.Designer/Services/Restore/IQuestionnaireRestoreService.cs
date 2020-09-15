using System;
using System.IO;

namespace WB.UI.Designer.Services.Restore
{
    public interface IQuestionnaireRestoreService
    {
        void RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state, Guid newQuestionnaireId);
    }
}
