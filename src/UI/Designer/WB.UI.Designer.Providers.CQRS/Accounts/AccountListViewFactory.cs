using WB.UI.Designer.Providers.CQRS.Accounts.View;
using WB.UI.Designer.Providers.Roles;
using Main.Core.Utility;
using Main.Core.View;
using Main.DenormalizerStorage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using WB.UI.Designer.Providers.CQRS.Accounts.View;
    using WB.UI.Designer.Providers.Roles;

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

            bool hasName = !string.IsNullOrEmpty(input.Name);
            bool hasEmail = !string.IsNullOrEmpty(input.Email);
            bool hasRole = input.Role != SimpleRoleEnum.Undefined;

            Func<AccountDocument, bool> query = (x) => true;

            if(hasRole && hasName)
            {
                query = (x) => x.SimpleRoles.Contains(input.Role) && x.UserName.Contains(input.Name);
            }
                else if (hasRole)
            {
                query = (x) => x.SimpleRoles.Contains(input.Role);
            }
            else if (input.IsNewOnly)
            {
                query = (x) => !x.IsConfirmed;
            }
            else if(input.IsOnlineOnly)
            {
                query = (x) => x.IsOnline;
            }
            else if(hasName)
            {
                query = (x) => x.UserName.Compare(input.Name);
            }
            else if (hasEmail)
            {
                query = (x) => x.Email.Compare(input.Email);
            }

            var queryResult = _accounts.Query().Where(query).AsQueryable().OrderUsingSortExpression(input.Order);

            retVal = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize)
                              .Select(x => new AccountListItem()
                                  {
                                      ApplicationName = x.ApplicationName,
                                      ProviderUserKey = x.ProviderUserKey,
                                      UserName = x.UserName,
                                      Comment = x.Comment,
                                      CreatedAt = x.CreatedAt,
                                      Email = x.Email,
                                      FailedPasswordAnswerWindowAttemptCount = x.FailedPasswordAnswerWindowAttemptCount,
                                      FailedPasswordAnswerWindowStartedAt = x.FailedPasswordAnswerWindowStartedAt,
                                      FailedPasswordWindowAttemptCount = x.FailedPasswordWindowAttemptCount,
                                      FailedPasswordWindowStartedAt = x.FailedPasswordWindowStartedAt,
                                      IsConfirmed = x.IsConfirmed,
                                      IsLockedOut = x.IsLockedOut,
                                      LastActivityAt = x.LastActivityAt,
                                      LastLockedOutAt = x.LastLockedOutAt,
                                      LastLoginAt = x.LastLoginAt,
                                      LastPasswordChangeAt = x.LastPasswordChangeAt
                                  });

            return new AccountListView(input.Page, input.PageSize, queryResult.Count(), retVal, input.Order);
        }

        #endregion
    }
}