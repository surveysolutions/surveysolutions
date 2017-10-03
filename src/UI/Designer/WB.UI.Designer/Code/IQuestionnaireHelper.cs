using System;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IQuestionnaireHelper
    {
        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid viewerId,
            bool isAdmin, 
            bool showPublic,
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string searchFor = null);

        IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId,
            bool isAdmin);
    }
}