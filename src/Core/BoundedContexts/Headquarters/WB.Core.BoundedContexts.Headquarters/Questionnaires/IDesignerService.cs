using WB.Core.BoundedContexts.Headquarters.DesignerPublicService;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires
{
    public interface IDesignerService
    {
        void TryLogin(string userName, string password);
        //QuestionnaireListViewMessage GetQuestionnaireList(DesignerQuestionnairesListModel questionnaireListRequest);
    }
}