// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResetPasswordConfirmationModel.cs" company="">
//   
// </copyright>
// <summary>
//   The reset password confirmation model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The reset password confirmation model.
    /// </summary>
    [DisplayName("Password reset")]
    public class ResetPasswordConfirmationModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password", Order = 1)]
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the confirm password.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password", Order = 2)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        #endregion
    }
}