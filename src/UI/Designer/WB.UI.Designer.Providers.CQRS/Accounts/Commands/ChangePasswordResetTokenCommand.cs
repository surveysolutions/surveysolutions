// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangePasswordResetTokenCommand.cs" company="">
//   
// </copyright>
// <summary>
//   The change password reset token command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Providers.CQRS.Accounts.Commands
{
    using System;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    ///     The change password reset token command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(AccountAR), "ChangePasswordResetToken")]
    public class ChangePasswordResetTokenCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordResetTokenCommand"/> class.
        /// </summary>
        public ChangePasswordResetTokenCommand()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordResetTokenCommand"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="passwordResetToken">
        /// The password reset token.
        /// </param>
        /// <param name="passwordResetExpirationDate">
        /// The password reset expiration date.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public ChangePasswordResetTokenCommand(
            Guid publicKey, string passwordResetToken, DateTime passwordResetExpirationDate)
        {
            AccountPublicKey = publicKey;
            PasswordResetToken = passwordResetToken;
            PasswordResetExpirationDate = passwordResetExpirationDate;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the account public key.
        /// </summary>
        [AggregateRootId]
        public Guid AccountPublicKey { get; set; }

        /// <summary>
        /// Gets or sets the password reset expiration date.
        /// </summary>
        public DateTime PasswordResetExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        public string PasswordResetToken { get; set; }

        #endregion
    }
}