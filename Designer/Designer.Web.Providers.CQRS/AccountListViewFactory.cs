using Main.Core.Utility;
using Main.Core.View;
using Main.DenormalizerStorage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Designer.Web.Providers.CQRS
{
    /// <summary>
    /// The account view factory.
    /// </summary>
    public class AccountListViewFactory : IViewFactory<AccountListViewInputModel, AccountListView>
    {
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<AccountDocument> _accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountListViewFactory"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The users.
        /// </param>
        public AccountListViewFactory(IDenormalizerStorage<AccountDocument> accounts)
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
        /// The RavenQuestionnaire.Core.Views.User.UserView.
        /// </returns>
        public AccountListView Load(AccountListViewInputModel input)
        {
            IEnumerable<AccountListItem> retVal = new AccountListItem[0];

            Func<AccountDocument, bool> query = (x) => false;
            if (input.IsNewOnly)
            {
                query = (x) => !x.IsConfirmed;
            }
            else if(input.IsOnlineOnly)
            {
                query = (x) => x.IsOnline;
            }
            else if(!string.IsNullOrEmpty(input.AccountName))
            {
                query = (x) => x.UserName.Compare(input.AccountName);
            }
            else if (!string.IsNullOrEmpty(input.AccountEmail))
            {
                query = (x) => x.Email.Compare(input.AccountEmail);
            }

            var orderedQuery = _accounts.Query()
                              .Where(query)
                              .Skip((input.Page - 1)*input.PageSize)
                              .Take(input.PageSize)
                              .Select(x => new AccountListItem()
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
                                  }).AsQueryable().OrderUsingSortExpression(input.Order);

            return new AccountListView(input.Page, input.PageSize, retVal.Count(), retVal, input.Order);
        }

        #endregion
    }
}