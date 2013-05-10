// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountModels.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   Defines the ChangePassword, LogOn and register Model  type for authorizing user.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Model for changing password
    /// </summary>
    public class ChangePasswordModel
    {
        /// <summary>
        /// Gets or sets OldPassword.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets NewPassword.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets ConfirmPassword.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Define LogOn model
    /// </summary>
    public class LogOnModel
    {
        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets Password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        
    }

    /// <summary>
    /// Define Register model
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets Email.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets Password.
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets ConfirmPassword.
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}