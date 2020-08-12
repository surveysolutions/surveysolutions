using System;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireListModel
    {
        public QuestionnaireListModel(bool isSupportAssignFolders, Guid? currentFolderId, 
            FolderBreadcrumbsModel[] breadcrumbs, IPagedList<QuestionnaireListViewModel> questionnaires)
        {
            IsSupportAssignFolders = isSupportAssignFolders;
            CurrentFolderId = currentFolderId;
            Breadcrumbs = breadcrumbs;
            Questionnaires = questionnaires;
        }

        public bool IsSupportAssignFolders { get; set; }
        public Guid? CurrentFolderId { get; set; }
        public FolderBreadcrumbsModel[] Breadcrumbs { get; set; }
        public IPagedList<QuestionnaireListViewModel> Questionnaires { get; set; }
    }
}
