using System;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireListModel
    {
        public Guid? CurrentFolderId { get; set; }
        public FolderBreadcrumbsModel[] Breadcrumbs { get; set; }
        public IPagedList<QuestionnaireListViewModel> Questionnaires { get; set; }
    }
}