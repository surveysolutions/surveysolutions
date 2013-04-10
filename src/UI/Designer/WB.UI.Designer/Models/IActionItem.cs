// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionItem.cs" company="">
//   
// </copyright>
// <summary>
//   The ActionItem interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    /// <summary>
    /// The ActionItem interface.
    /// </summary>
    public interface IActionItem
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether can copy.
        /// </summary>
        bool CanCopy { get; }

        /// <summary>
        /// Gets a value indicating whether can delete.
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// Gets a value indicating whether can edit.
        /// </summary>
        bool CanEdit { get; }

        /// <summary>
        /// Gets a value indicating whether can export.
        /// </summary>
        bool CanExport { get; }

        /// <summary>
        /// Gets a value indicating whether can preview.
        /// </summary>
        bool CanPreview { get; }

        /// <summary>
        /// Gets a value indicating whether can print.
        /// </summary>
        bool CanPrint { get; }

        /// <summary>
        /// Gets a value indicating whether can synchonize.
        /// </summary>
        bool CanSynchronize { get; }

        #endregion
    }
}