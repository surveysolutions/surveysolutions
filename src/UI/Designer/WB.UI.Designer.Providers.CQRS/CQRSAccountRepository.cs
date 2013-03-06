using System;
using System.Collections.Generic;
using WB.UI.Designer.Providers.CQRS.Accounts.Commands;
using WB.UI.Designer.Providers.CQRS.Accounts.View;
using WB.UI.Designer.Providers.Membership;
using Main.Core.View;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using System.Web.Security;

namespace WB.UI.Designer.Providers.CQRS
{
    using WB.UI.Designer.Providers.CQRS.Accounts.Commands;
    using WB.UI.Designer.Providers.CQRS.Accounts.View;
    using WB.UI.Designer.Providers.Membership;

    public class CQRSAccountRepository : IAccountRepository
    {
        private readonly IViewRepository _viewRepository;
        private readonly ICommandService _commandService;
        
        public CQRSAccountRepository(IViewRepository repository, ICommandService commandService)
        {
            _viewRepository = repository;
            _commandService = commandService;
        }

        public bool IsUniqueEmailRequired { get; set; }

        public IMembershipAccount Get(string username)
        {
            if (username == null) throw new ArgumentNullException("username");

            return GetUser(accountName: username, accountEmail: null, confirmationToken: null);
        }

        public IMembershipAccount GetByProviderKey(object id)
        {
            if (id == null) throw new ArgumentNullException("id");

            return _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(id));
        }

        public string GetUserNameByEmail(string email)
        {
            if (email == null) throw new ArgumentNullException("email");

            var account = GetUser(accountName: null, accountEmail: email, confirmationToken: null);
            return account == null ? string.Empty : account.UserName;
        }

        public int GetNumberOfUsersOnline()
        {
            var accountsOnline = _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
                {
                    IsOnlineOnly = true
                });
            return accountsOnline.TotalCount;
        }

        public IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords)
        {
            var accounts = _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
            {
                Page =  pageIndex,
                PageSize = pageSize
            });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords)
        {
            var accounts = _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
            {
                IsNewOnly = true,
                Page = pageIndex,
                PageSize = pageSize
            });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindByUserName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var accounts = _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
            {
                Name = usernameToMatch,
                Page = pageIndex,
                PageSize = pageSize
            });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public IEnumerable<IMembershipAccount> FindByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var accounts = _viewRepository.Load<AccountListViewInputModel, AccountListView>(new AccountListViewInputModel()
            {
                Email = emailToMatch,
                Page = pageIndex,
                PageSize = pageSize
            });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            var account = GetUser(accountName: null, accountEmail: null, confirmationToken: confirmationToken);
            return account == null ? string.Empty : account.UserName;
        }

        public MembershipCreateStatus Register(IMembershipAccount account)
        {
            _commandService.Execute(new RegisterAccountCommand(
                                        applicationName: account.ApplicationName, userName: account.UserName,
                                        email: account.Email,
                                        providerUserKey: account.ProviderUserKey,
                                        password: account.Password,
                                        passwordSalt: account.PasswordSalt,
                                        isConfirmed: account.IsConfirmed,
                                        confirmationToken: account.ConfirmationToken));
            return MembershipCreateStatus.Success;
        }

        public void Update(IMembershipAccount account, MembershipEventType eventType)
        {
            var accountPublicKey = (Guid)account.ProviderUserKey;

            ICommand command = null;

            switch (eventType)
            {
                case MembershipEventType.ConfirmAccount:
                    command = new ConfirmAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.ChangePassword:
                    command = new ChangePasswordAccountCommand(publicKey: accountPublicKey, password: account.Password);
                    break;
                case MembershipEventType.ChangePasswordQuestionAndAnswer:
                    command = new ChangePasswordQuestionAndAnswerAccountCommand(publicKey: accountPublicKey,
                                                                                passwordQuestion: account.PasswordQuestion,
                                                                                passwordAnswer: account.PasswordAnswer);
                    break;
                case MembershipEventType.LockUser:
                    command = new LockAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.ResetPassword:
                    command = new ResetPasswordAccountCommand(publicKey: accountPublicKey, password: account.Password,
                                                              passwordSalt: account.PasswordSalt);
                    break;
                case MembershipEventType.UnlockUser:
                    command = new UnlockAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.Update:
                    command = new UpdateAccountCommand(publicKey: accountPublicKey, userName: account.UserName,
                                                       isLockedOut: account.IsLockedOut,
                                                       passwordQuestion: account.PasswordQuestion, email: account.Email,
                                                       isConfirmed: account.IsConfirmed, comment: account.Comment);
                    break;
                case MembershipEventType.UpdateOnlineState:
                    command = new ChangeOnlineAccountCommand(accountPublicKey);
                    break;
                case MembershipEventType.UserValidated:
                    command = new ValidateAccountCommand(accountPublicKey);
                    break;
                    case MembershipEventType.FailedLogin:
                    command = new LoginFailedAccountCommand(accountPublicKey);
                    break;

            }

            if (command != null)
            {
                _commandService.Execute(command);
            }
        }

        public bool Delete(string username, bool deleteAllRelatedData)
        {
            var account = GetUser(accountName: username, accountEmail: null, confirmationToken: null);

            _commandService.Execute(new DeleteAccountCommand(account.PublicKey));

            return GetUser(accountName: username, accountEmail: null, confirmationToken: null) == null;
        }

        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            var account = new AccountView
            {
                ProviderUserKey = Guid.NewGuid(),
                ApplicationName = applicationName,
                UserName = username,
                Email = email
            };
            return account;
        }

        private AccountView GetUser(string accountName, string accountEmail, string confirmationToken)
        {
            return
                _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(
                                                                             accountName: accountName,
                                                                             accountEmail: accountEmail,
                                                                             confirmationToken: confirmationToken));
        }
    }
}
