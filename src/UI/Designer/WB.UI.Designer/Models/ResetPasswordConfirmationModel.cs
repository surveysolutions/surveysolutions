// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResetPasswordConfirmationModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.Web.Mvc;

    /// <summary>
    ///     The reset password confirmation model.
    /// </summary>
    [DisplayName("Password reset")]
    public class ResetPasswordConfirmationModel : PasswordModel
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the token.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public string Token { get; set; }

        #endregion
    }
}