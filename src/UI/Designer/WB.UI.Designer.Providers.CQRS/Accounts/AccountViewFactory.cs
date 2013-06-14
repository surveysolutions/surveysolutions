// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountViewFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The account view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using System;
    using System.Linq;

    using Main.Core.Utility;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    using WB.UI.Designer.Providers.CQRS.Accounts.View;

    /// <summary>
    ///     The account view factory.
    /// </summary>
    public class AccountViewFactory : IViewFactory<AccountViewInputModel, AccountView>
    {
        #region Fields

        /// <summary>
        ///     The accounts.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<AccountDocument> accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountViewFactory"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The accounts.
        /// </param>
        public AccountViewFactory(IQueryableDenormalizerStorage<AccountDocument> accounts)
        {
            this.accounts = accounts;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The WB.UI.Designer.Providers.CQRS.AccountView.
        /// </returns>
        public AccountView Load(AccountViewInputModel input)
        {
            Func<AccountDocument, bool> query = (x) => false;
            if (input.ProviderUserKey != null)
            {
                query = (x) => x.ProviderUserKey.Equals(input.ProviderUserKey);
            }
            else if (!string.IsNullOrEmpty(input.AccountName))
            {
                query = (x) => x.UserName.Compare(input.AccountName);
            }
            else if (!string.IsNullOrEmpty(input.AccountEmail))
            {
                query = (x) => x.Email.Compare(input.AccountEmail);
            }
            else if (!string.IsNullOrEmpty(input.ConfirmationToken))
            {
                query = (x) => x.ConfirmationToken == input.ConfirmationToken;
            }
            else if (!string.IsNullOrEmpty(input.ResetPasswordToken))
            {
                query = (x) => x.PasswordResetToken == input.ResetPasswordToken;
            }

            return
                this.accounts.Query(_ => _
                    .Where(query)
                    .Select(
                        x =>
                        new AccountView
                            {
                                ApplicationName = x.ApplicationName, 
                                ProviderUserKey = x.ProviderUserKey, 
                                UserName = x.UserName, 
                                Comment = x.Comment, 
                                ConfirmationToken = x.ConfirmationToken, 
                                CreatedAt = x.CreatedAt, 
                                Email = x.Email, 
                                FailedPasswordAnswerWindowAttemptCount =
                                    x.FailedPasswordAnswerWindowAttemptCount, 
                                FailedPasswordAnswerWindowStartedAt = x.FailedPasswordAnswerWindowStartedAt, 
                                FailedPasswordWindowAttemptCount = x.FailedPasswordWindowAttemptCount, 
                                FailedPasswordWindowStartedAt = x.FailedPasswordWindowStartedAt, 
                                IsConfirmed = x.IsConfirmed, 
                                IsLockedOut = x.IsLockedOut, 
                                IsOnline = x.IsOnline, 
                                LastActivityAt = x.LastActivityAt, 
                                LastLockedOutAt = x.LastLockedOutAt, 
                                LastLoginAt = x.LastLoginAt, 
                                LastPasswordChangeAt = x.LastPasswordChangeAt, 
                                Password = x.Password, 
                                PasswordSalt = x.PasswordSalt, 
                                PasswordQuestion = x.PasswordQuestion, 
                                PasswordAnswer = x.PasswordAnswer, 
                                PasswordResetToken = x.PasswordResetToken, 
                                PasswordResetExpirationDate = x.PasswordResetExpirationDate, 
                                SimpleRoles = x.SimpleRoles
                            })
                    .FirstOrDefault());
        }

        #endregion
    }
}