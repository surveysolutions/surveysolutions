using System;
using System.IO;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Code
{
    public interface IQuestionnaireHelper
    {
        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            DesignerIdentityUser viewer,
            bool isAdmin,
            QuestionnairesType type,
            Guid? folderId,
            int? pageIndex = null, 
            string? sortBy = null, 
            int? sortOrder = null, 
            string? searchFor = null);

        IPagedList<QuestionnaireListViewModel> GetMyQuestionnairesByViewerId(DesignerIdentityUser viewer, bool isAdmin, Guid? folderId);

        IPagedList<QuestionnaireListViewModel> GetSharedQuestionnairesByViewer(DesignerIdentityUser viewer, bool isAdmin, Guid? folderId);
        
        Stream? GetBackupQuestionnaire(QuestionnaireRevision id, out string questionnaireFileName);
    }
}
