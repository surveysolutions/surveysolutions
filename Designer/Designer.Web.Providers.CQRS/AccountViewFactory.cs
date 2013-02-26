using Main.Core.View;
using Main.DenormalizerStorage;
using System;
using System.Linq;
using Main.Core.Utility;

namespace Designer.Web.Providers.CQRS
{
    /// <summary>
    /// The account view factory.
    /// </summary>
    public class AccountViewFactory : IViewFactory<AccountViewInputModel, AccountView>
    {
        #region Fields

        /// <summary>
        /// The accounts.
        /// </summary>
        private readonly IDenormalizerStorage<AccountDocument> _accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountViewFactory"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The accounts.
        /// </param>
        public AccountViewFactory(IDenormalizerStorage<AccountDocument> accounts)
        {
            _accounts = accounts;
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
        /// The Designer.Web.Providers.CQRS.AccountView.
        /// </returns>
        public AccountView Load(AccountViewInputModel input)
        {
            Func<AccountDocument,bool> query = (x) => false;
            if (input.AccountId != Guid.Empty)
            {
                query = (x) => (Guid) x.ProviderUserKey == input.AccountId;
            }
            else if(!string.IsNullOrEmpty( input.AccountName))
            {
                query = (x) => x.UserName.Compare(input.AccountName);
            }
            else if(!string.IsNullOrEmpty(input.AccountEmail))
            {
                query = (x) => x.Email.Compare(input.AccountEmail);
            }
            else if(!string.IsNullOrEmpty(input.ConfirmationToken))
            {
                query = (x) => x.ConfirmationToken == input.ConfirmationToken;
            }

            return _accounts.Query().Where(query).Select(x=>new AccountView()
                {
                    ApplicationName = x.ApplicationName,
                    ProviderUserKey = x.ProviderUserKey,
                    UserName = x.UserName,
                    Comment = x.Comment,
                    ConfirmationToken = x.ConfirmationToken,
                    CreatedAt = x.CreatedAt,
                    Email = x.Email,
                    FailedPasswordAnswerWindowAttemptCount = x.FailedPasswordAnswerWindowAttemptCount,
                    FailedPasswordAnswerWindowStartedAt = x.FailedPasswordAnswerWindowStartedAt,
                    FailedPasswordWindowAttemptCount = x.FailedPasswordWindowAttemptCount,
                    FailedPasswordWindowStartedAt = x.FailedPasswordWindowStartedAt,
                    IsConfirmed = x.IsConfirmed,
                    IsLockedOut = x.IsLockedOut,
                    IsOnline = x.IsOnline,
                    LastActivityAt = x.LastActivityAt,
                    LastLockedOutAt = x.LastLockedOutAt,
                    LastLoginAt = x.LastLoginAt,
                    LastPasswordChangeAt = x.LastPasswordChangeAt
                }).FirstOrDefault();
        }

        #endregion
    }
}