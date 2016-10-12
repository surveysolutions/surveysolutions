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
        private readonly IAccountListViewFactory accountListViewFactory;
        private readonly IAccountViewFactory accountViewFactory;

        public CQRSAccountRepository(ICommandService commandService, IAccountListViewFactory accountListViewFactory, IAccountViewFactory accountViewFactory)
        {
            this.commandService = commandService;
            this.accountListViewFactory = accountListViewFactory;
            this.accountViewFactory = accountViewFactory;
        }

        public bool IsUniqueEmailRequired { get; set; }

        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            var account = new Aggregates.Account
                              {
                                  ProviderUserKey = Guid.Parse(providerUserKey.ToString()), 
                                  ApplicationName = applicationName, 
                                  UserName = username, 
                                  Email = email
                              };
            return account;
        }

        public bool Delete(string username, bool deleteAllRelatedData)
        {
            IAccountView account = this.GetUser(accountName: username);

            this.commandService.Execute(new DeleteAccountCommand(account.ProviderUserKey));

            return this.GetUser(accountName: username) == null;
        }

        public IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindByEmail(
            string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Email = emailToMatch, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindByUserName(
            string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { Name = usernameToMatch, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords)
        {
            AccountListView accounts =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { IsNewOnly = true, Page = pageIndex, PageSize = pageSize });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IMembershipAccount Get(string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }

            return this.GetUser(accountName: username);
        }

        public IMembershipAccount GetByProviderKey(object id)
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            return this.accountViewFactory.Load(new AccountViewInputModel(Guid.Parse(id.ToString())));
        }

        public int GetNumberOfUsersOnline()
        {
            AccountListView accountsOnline =
                this.accountListViewFactory.Load(
                    new AccountListViewInputModel { IsOnlineOnly = true });
            return accountsOnline.TotalCount;
        }

        public IMembershipAccount GetUserByResetPasswordToken(string token)
        {
            return this.GetUser(resetPasswordToken: token);
        }

        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            IAccountView account = this.GetUser(confirmationToken: confirmationToken);
            return account == null ? string.Empty : account.UserName;
        }

        public string GetUserNameByEmail(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            IAccountView account = this.GetUser(accountEmail: email);
            return account == null ? string.Empty : account.UserName;
        }

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

        private IAccountView GetUser(
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
    }
}