using System;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IQuestionnaireHelper
    {
        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            string viewerId,
            bool isAdmin,
            QuestionnairesType type,
            Guid? folderId,
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string searchFor = null);

        IPagedList<QuestionnaireListViewModel> GetMyQuestionnairesByViewerId(string viewerId, bool isAdmin, Guid? folderId);

        IPagedList<QuestionnaireListViewModel> GetSharedQuestionnairesByViewerId(string viewerId, bool isAdmin, Guid? folderId);
    }
}
