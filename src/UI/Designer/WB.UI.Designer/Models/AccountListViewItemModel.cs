// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountListViewItemModel.cs" company="">
//   
// </copyright>
// <summary>
//   The account list view item model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The account list view item model.
    /// </summary>
    [DisplayName("account")]
    public class AccountListViewItemModel : ActionItem
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        [Display(Name = "Created date", Order = 4)]
        public string CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is approved.
        /// </summary>
        [Display(Name = "Approved?", Order = 5)]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked out.
        /// </summary>
        [Display(Name = "Locked?", Order = 5)]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        [Display(Name = "Last login", Order = 3)]
        public string LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Display(Name = "Name", Order = 1)]
        [Default]
        public string UserName { get; set; }

        /// <summary>
        ///     Gets a value indicating whether can copy.
        /// </summary>
        public override bool CanCopy
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether can export.
        /// </summary>
        public override bool CanExport
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}