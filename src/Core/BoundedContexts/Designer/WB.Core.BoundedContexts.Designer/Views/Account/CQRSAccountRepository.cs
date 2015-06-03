using System;
using System.Collections.Generic;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Commands.Account;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Shared.Web.MembershipProvider.Accounts;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public class CQRSAccountRepository : IAccountRepository
    {
        private readonly ICommandService commandService;
        private readonly IViewFactory<AccountListViewInputModel, AccountListView> accountListViewFactory;
        private readonly IViewFactory<AccountViewInputModel, AccountView> accountViewFactory;

        public CQRSAccountRepository(ICommandService commandService, IViewFactory<AccountListViewInputModel, AccountListView> accountListViewFactory, IViewFactory<AccountViewInputModel, AccountView> accountViewFactory)
        {
            this.commandService = commandService;
            this.accountListViewFactory = accountListViewFactory;
            this.accountViewFactory = accountViewFactory;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether is unique email required.
        /// </summary>
        public bool IsUniqueEmailRequired { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="applicationName">
        /// The application name.
        /// </param>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="IMembershipAccount"/>.
        /// </returns>
        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            var account = new AccountView
                              {
                                  ProviderUserKey = Guid.Parse(providerUserKey.ToString()), 
                                  ApplicationName = applicationName, 
                                  UserName = username, 
                                  Email = email
                              };
            return account;
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="deleteAllRelatedData">
        /// The delete all related data.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Delete(string username, bool deleteAllRelatedData)
        {
            AccountView account = this.GetUser(accountName: username);

            this.commandService.Execute(new DeleteAccountCommand(account.ProviderUserKey));

            return this.GetUser(accountName: username) == null;
        }

        /// <summary>
        /// The find all.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        /// <summary>
        /// The find by email.
        /// </summary>
        /// <param name="emailToMatch">
        /// The email to match.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<IMembershipAccount> FindByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Email = emailToMatch, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        /// <summary>
        /// The find by user name.
        /// </summary>
        /// <param name="usernameToMatch">
        /// The username to match.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<IMembershipAccount> FindByUserName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Name = usernameToMatch, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        /// <summary>
        /// The find new accounts.
        /// </summary>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="totalRecords">
        /// The total records.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>IEnumerable</cref>
        ///     </see>
        ///     .
        /// </returns>
        public IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { IsNewOnly = true, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <returns>
        /// The <see cref="IMembershipAccount"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public IMembershipAccount Get(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }

            return this.GetUser(accountName: username);
        }

        /// <summary>
        /// The get by provider key.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="IMembershipAccount"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public IMembershipAccount GetByProviderKey(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            return this.accountViewFactory.Load(new AccountViewInputModel(Guid.Parse(id.ToString())));
        }

        /// <summary>
        /// The get number of users online.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetNumberOfUsersOnline()
        {
            AccountListView accountsOnline =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { IsOnlineOnly = true });
            return accountsOnline.TotalCount;
        }

        /// <summary>
        /// The get user by reset password token.
        /// </summary>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// The <see cref="IMembershipAccount"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public IMembershipAccount GetUserByResetPasswordToken(string token)
        {
            return this.GetUser(resetPasswordToken: token);
        }

        /// <summary>
        /// The get user name by confirmation token.
        /// </summary>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            AccountView account = this.GetUser(confirmationToken: confirmationToken);
            return account == null ? string.Empty : account.UserName;
        }

        /// <summary>
        /// The get user name by email.
        /// </summary>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public string GetUserNameByEmail(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            AccountView account = this.GetUser(accountEmail: email);
            return account == null ? string.Empty : account.UserName;
        }

        /// <summary>
        /// The register.
        /// </summary>
        /// <param name="account">
        /// The account.
        /// </param>
        /// <returns>
        /// The <see cref="System.Web.Security.MembershipCreateStatus"/>.
        /// </returns>
        public MembershipCreateStatus Register(IMembershipAccount account)
        {
            this.commandService.Execute(
                new RegisterAccountCommand(
                    applicationName: account.ApplicationName, 
                    userName: account.UserName, 
                    email: account.Email, 
                    accountId: account.ProviderUserKey, 
                    password: account.Password, 
                    passwordSalt: account.PasswordSalt, 
                    isConfirmed: account.IsConfirmed, 
                    confirmationToken: account.ConfirmationToken));
            return MembershipCreateStatus.Success;
        }

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="account">
        /// The account.
        /// </param>
        /// <param name="eventType">
        /// The event type.
        /// </param>
        public void Update(IMembershipAccount account, MembershipEventType eventType)
        {
            var accountPublicKey = account.ProviderUserKey;

            ICommand command = null;

            switch (eventType)
            {
                case MembershipEventType.ConfirmAccount:
                    command = new ConfirmAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.ChangePassword:
                    command = new ChangePasswordAccountCommand(accountId: accountPublicKey, password: account.Password);
                    break;
                case MembershipEventType.ChangePasswordQuestionAndAnswer:
                    command = new ChangePasswordQuestionAndAnswerAccountCommand(
                        accountId: accountPublicKey, 
                        passwordQuestion: account.PasswordQuestion, 
                        passwordAnswer: account.PasswordAnswer);
                    break;
                case MembershipEventType.LockUser:
                    command = new LockAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.ResetPassword:
                    command = new ResetPasswordAccountCommand(
                        accountId: accountPublicKey, password: account.Password, passwordSalt: account.PasswordSalt);
                    break;
                case MembershipEventType.UnlockUser:
                    command = new UnlockAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.Update:
                    command = new UpdateAccountCommand(
                        accountId: accountPublicKey, 
                        userName: account.UserName, 
                        isLockedOut: account.IsLockedOut, 
                        passwordQuestion: account.PasswordQuestion, 
                        email: account.Email, 
                        isConfirmed: account.IsConfirmed, 
                        comment: account.Comment);
                    break;
                case MembershipEventType.UpdateOnlineState:
                    //command = new ChangeOnlineAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.UserValidated:
                    command = new ValidateAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.FailedLogin:
                    command = new LoginFailedAccountCommand(accountPublicKey);
                    break;
                    case MembershipEventType.ChangePasswordResetToken:
                    command = new ChangePasswordResetTokenCommand(
                        accountId: accountPublicKey,
                        passwordResetToken: account.PasswordResetToken,
                        passwordResetExpirationDate: account.PasswordResetExpirationDate);
                    break;
            }

            if (command != null)
            {
                this.commandService.Execute(command);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get user.
        /// </summary>
        /// <param name="accountName">
        /// The account name.
        /// </param>
        /// <param name="accountEmail">
        /// The account email.
        /// </param>
        /// <param name="confirmationToken">
        /// The confirmation token.
        /// </param>
        /// <param name="resetPasswordToken">
        /// The reset password token.
        /// </param>
        /// <returns>
        /// The <see cref="WB.Core.BoundedContexts.Designer.Views.Account.AccountView"/>.
        /// </returns>
        private AccountView GetUser(
            string accountName = null, 
            string accountEmail = null, 
            string confirmationToken = null, 
            string resetPasswordToken = null)
        {
            return
                this.accountViewFactory.Load(
                    new AccountViewInputModel(
                        accountName: accountName, 
                        accountEmail: accountEmail, 
                        confirmationToken: confirmationToken, 
                        resetPasswordToken: resetPasswordToken));
        }

        #endregion
    }
}