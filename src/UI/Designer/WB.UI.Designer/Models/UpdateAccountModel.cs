namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    /// <summary>
    /// The update account model.
    /// </summary>
    [DisplayName("Update Account Info")]
    public class UpdateAccountModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        [Display(Name = "Comment", Order = 5)]
        [DataType(DataType.MultilineText)]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 2)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is approved.
        /// </summary>
        [Display(Name = "Is Approved", Order = 3)]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked out.
        /// </summary>
        [Display(Name = "Is Locked Out", Order = 4)]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Display(Name = "User name", Order = 1)]
        [ReadOnly(true)]
        public string UserName { get; set; }

        #endregion
    }
}