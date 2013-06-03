// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SystemMailer.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Mailers
{
    using Mvc.Mailer;

    using WB.UI.Designer.Models;

    /// <summary>
    ///     The system mailer.
    /// </summary>
    public class SystemMailer : MailerBase, ISystemMailer
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SystemMailer" /> class.
        /// </summary>
        public SystemMailer()
        {
            this.MasterName = "_Layout";
        }

        #endregion

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
        public virtual MvcMailMessage ConfirmationEmail(EmailConfirmationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                    {
                        x.Subject = "Complete Registration Process";
                        x.ViewName = "ConfirmationEmail";
                        x.To.Add(model.Email);
                    });
        }

        /// <summary>
        /// The reset password email.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="MvcMailMessage"/>.
        /// </returns>
        public virtual MvcMailMessage ResetPasswordEmail(EmailConfirmationModel model)
        {
            this.ViewData.Model = model;
            return this.Populate(
                x =>
                    {
                        x.Subject = "Complete Password Reset";
                        x.ViewName = "ResetPasswordEmail";
                        x.To.Add(model.Email);
                    });
        }

        #endregion
    }
}