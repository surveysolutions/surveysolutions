using System;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires
{
    public interface IDesignerService
    {
        void TryLogin(string userName, string password);

        QuestionnaireListDto GetQuestionnaireList(string filter, int page, int pageSize, string sortOrder);
        RemoteFileInfo DownloadQuestionnaire(Guid questionnaireId, SharedKernels.QuestionnaireVerification.ValueObjects.QuestionnaireVersion version);
        bool CurrentUserIsLoggedIn();
    }
}