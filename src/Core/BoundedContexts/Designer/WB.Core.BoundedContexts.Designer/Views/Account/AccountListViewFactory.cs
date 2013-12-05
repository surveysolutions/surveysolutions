﻿using System.Collections.Generic;
using System.Linq;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
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
        private readonly IQueryableReadSideRepositoryReader<AccountDocument> _accounts;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountListViewFactory"/> class.
        /// </summary>
        /// <param name="accounts">
        /// The users.
        /// </param>
        public AccountListViewFactory(IQueryableReadSideRepositoryReader<AccountDocument> accounts)
        {
            this._accounts = accounts;
        }

        #endregion

        public AccountListView Load(AccountListViewInputModel input)
        {
            IEnumerable<AccountListItem> retVal = new AccountListItem[0];

            var count =
                this._accounts.Query(_ => this.FilterAccounts(_, input).Count());

            var queryResult = this._accounts.Query(_ => this.FilterAccounts(_, input).OrderUsingSortExpression(input.Order).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList());

            retVal = queryResult
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

            return new AccountListView(input.Page, input.PageSize, count, retVal, input.Order);
        }

        private IQueryable<AccountDocument> FilterAccounts(IQueryable<AccountDocument> accounts, AccountListViewInputModel input)
        {
            IQueryable<AccountDocument> result = accounts;
            bool hasName = !string.IsNullOrEmpty(input.Name);
            bool hasEmail = !string.IsNullOrEmpty(input.Email);
            bool hasRole = input.Role != SimpleRoleEnum.Undefined;

            if (hasRole && hasName)
            {
                result = result.Where((x) => x.SimpleRoles.Any(r => r == input.Role) && x.UserName == input.Name);
            }
            else if (hasRole)
            {
                result = result.Where((x) => x.SimpleRoles.Any(r => r == input.Role));
            }
            else if (input.IsNewOnly)
            {
               result = result.Where((x) => !x.IsConfirmed);
            }
            else if (input.IsOnlineOnly)
            {
                result = result.Where((x) => x.IsOnline);
            }
            else if (hasName)
            {
                result = result.Where((x) => x.UserName == input.Name);
            }
            else if (hasEmail)
            {
                result = result.Where( (x) => x.Email == input.Email);
            }
            return result;
        }
    }
}