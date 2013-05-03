// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegisterModel.cs" company="">
//   
// </copyright>
// <summary>
//   The register model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The register model.
    /// </summary>
    [DisplayName("Registration")]
    public class RegisterModel : PasswordModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Required]
        [Display(Name = "User name", Order = 1)]
        [RegularExpression("^[a-z0-9_]{3,15}$", ErrorMessage = "User name needs to be between 3 and 15 characters and contains only letters, digits and symbol \"_\".")]
        public string UserName { get; set; }

        #endregion
    }
}