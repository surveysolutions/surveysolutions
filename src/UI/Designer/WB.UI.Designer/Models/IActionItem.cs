namespace WB.UI.Designer.Models
{
    public interface IActionItem
    {
        bool CanCopy { get; }

        bool CanDelete { get; }

        bool CanEdit { get; }

        bool CanExport { get; }

        bool CanPreview { get; }

        bool CanPrint { get; }

        bool CanSynchronize { get; }

        bool CanExportToPdf { get; }
    }
}