using System;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IQuestionnaireHelper
    {
        IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            Guid viewerId,
            bool isAdmin, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid viewerId,
            bool isAdmin, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId,
            bool isAdmin);
    }
}