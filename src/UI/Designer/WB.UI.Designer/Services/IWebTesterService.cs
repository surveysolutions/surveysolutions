using System;

namespace WB.UI.Designer.Services
{
    public interface IWebTesterService
    {
        Guid? GetQuestionnaire(string token);
        string CreateTestQuestionnaire(Guid questionnaireId);
        void StartBackgroundCleanupJob();
    }
}