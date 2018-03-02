namespace WB.UI.Designer.Models
{
    public abstract class ActionItem : IActionItem
    {
        public ActionItem()
        {
            CanDelete = false;
            CanEdit = false;
            CanPreview = false;
            CanExport = false;
            CanSynchronize = false;
            CanOpen = false;
            CanAssignFolder = false;
        }

        public virtual bool CanDelete { get; set; }

        public virtual bool CanEdit { get; set; }

        public virtual bool CanOpen { get; set; }

        public virtual bool CanPreview { get; set; }

        public virtual bool CanExport { get; set; }

        public virtual bool CanSynchronize { get; set; }

        public bool CanExportToPdf { get; set; }

        public bool CanExportToHtml { get; set; }

        public virtual bool CanAssignFolder { get; set; }

        public virtual bool CanCopy { get; set; }

        public virtual bool CanPrint
        {
            get
            {
                return false;
            }
        }
    }
}