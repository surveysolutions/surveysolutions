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
    public class RegisterModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the confirm password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 3)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email", Order = 4)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Required]
        [Display(Name = "User name", Order = 1)]
        [RegularExpression("^[a-zA-Z0-9_]{3,15}$", ErrorMessage = "User name needs to be between 3 and 15 characters and contains only letters, digits and underscore symbol")]
        public string UserName { get; set; }

        #endregion
    }
}