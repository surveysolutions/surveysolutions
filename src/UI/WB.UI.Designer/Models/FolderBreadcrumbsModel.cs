using System;

namespace WB.UI.Designer.Models
{
    public class FolderBreadcrumbsModel
    {
        public FolderBreadcrumbsModel(Guid folderId, string title)
        {
            FolderId = folderId;
            Title = title;
        }

        public Guid? FolderId { get; set; }
        public string Title { get; set; }
    }
}
