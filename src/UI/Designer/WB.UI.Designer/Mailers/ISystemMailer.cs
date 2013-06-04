// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISystemMailer.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Mailers
{
    using Mvc.Mailer;

    using WB.UI.Designer.Models;

    /// <summary>
    /// The SystemMailer interface.
    /// </summary>
    public interface ISystemMailer
    {
        #region Public Methods and Operators

        /// <summary>
        /// The confirmation email.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="MvcMailMessage"/>.
        /// </returns>
        MvcMailMessage ConfirmationEmail(EmailConfirmationModel model);

        /// <summary>
        /// The reset password email.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="MvcMailMessage"/>.
        /// </returns>
        MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model);

        #endregion
    }
}