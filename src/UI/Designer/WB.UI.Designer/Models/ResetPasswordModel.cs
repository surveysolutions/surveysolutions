namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The reset password model.
    /// </summary>
    [DisplayName("Password reset")]
    public class ResetPasswordModel
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        [Required]
        [Display(Name = "User Name", Order = 1)]
        public string UserName { get; set; }

        #endregion
    }
}