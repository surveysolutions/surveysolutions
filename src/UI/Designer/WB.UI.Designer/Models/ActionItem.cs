
namespace WB.UI.Designer.Models
{
    public abstract class ActionItem : IActionItem
    {
        public ActionItem()
        {
            CanDelete = false;
            CanEdit = false;
            CanPreview = false;
        }

        /// <summary>
        ///     Gets a value indicating whether can copy.
        /// </summary>
        public virtual bool CanCopy
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether can delete.
        /// </summary>
        public virtual bool CanDelete { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can edit.
        /// </summary>
        public virtual bool CanEdit { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can preview.
        /// </summary>
        public virtual bool CanPreview { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can export.
        /// </summary>
        public virtual bool CanExport
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether can print.
        /// </summary>
        public virtual bool CanPrint
        {
            get
            {
                return false;
            }
        }
    }
}