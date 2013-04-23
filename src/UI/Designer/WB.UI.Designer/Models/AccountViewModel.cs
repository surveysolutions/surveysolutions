// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The account view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The account view model.
    /// </summary>
    [DisplayName("Account")]
    public class AccountViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        [Display(Name = "Comment", Order = 12)]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        [Display(Name = "Created date", Order = 3)]
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
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is approved.
        /// </summary>
        [Display(Name = "Is Approved", Order = 5)]
        public bool IsApproved { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked out.
        /// </summary>
        [Display(Name = "Is Locked Out", Order = 6)]
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is online.
        /// </summary>
        //[Display(Name = "Is Online", Order = 7)]
        //public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the last activity date.
        /// </summary>
        //[Display(Name = "Last Activity Date", Order = 8)]
        //public string LastActivityDate { get; set; }

        /// <summary>
        /// Gets or sets the last lockout date.
        /// </summary>
        [Display(Name = "Last Lockout Date", Order = 9)]
        public string LastLockoutDate { get; set; }

        /// <summary>
        /// Gets or sets the last login date.
        /// </summary>
        [Display(Name = "Last login", Order = 4)]
        public string LastLoginDate { get; set; }

        /// <summary>
        /// Gets or sets the last password changed date.
        /// </summary>
        [Display(Name = "Last Password Changed Date", Order = 10)]
        public string LastPasswordChangedDate { get; set; }

        /// <summary>
        ///     Gets or sets the questionnaires.
        /// </summary>
        [Display(Name = "Questionnaires", Order = 13)]
        [UIHint("Admin_QiestionnaireListView")]
        public IEnumerable<QuestionnaireListViewModel> Questionnaires { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Display(Name = "Name", Order = 1)]
        public string UserName { get; set; }

        #endregion
    }
}