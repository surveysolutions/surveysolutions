namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The login model.
    /// </summary>
    [DisplayName("Please log in")]
    public class LoginModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 2)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether remember me.
        /// </summary>
        [Display(Name = "Remember me?", Order = 3)]
        public bool RememberMe { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Required]
        [Display(Name = "User name", Order = 1)]
        public string UserName { get; set; }

        #endregion
    }
}