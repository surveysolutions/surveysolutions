using System;
using System.Collections.Generic;
using Designer.Web.Providers.Membership;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using System.Web.Security;

namespace Designer.Web.Providers.CQRS
{
    public class CQRSAccountRepository : IAccountRepository
    {
        private readonly IViewRepository _viewRepository;
        private readonly ICommandService _commandService;

        public bool IsEventSourcingUsed
        {
            get { return true; }
        }

        public CQRSAccountRepository(IViewRepository repository, ICommandService commandService)
        {
            _viewRepository = repository;
            _commandService = commandService;
        }

        public bool IsUniqueEmailRequired { get; set; }

        public IMembershipAccount Get(string username)
        {
            return
                _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(
                                                                             accountName: username, accountEmail: null,
                                                                             confirmationToken: null));
        }

        public IMembershipAccount GetByProviderKey(object id)
        {
            return _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel((Guid) id));
        }

        public string GetUserNameByEmail(string email)
        {
            var account =
                _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(accountName: null,
                                                                                                   accountEmail: email,
                                                                                                   confirmationToken: null));
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
                AccountName = usernameToMatch,
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
                AccountEmail = emailToMatch,
                Page = pageIndex,
                PageSize = pageSize
            });
            totalRecords = accounts.TotalCount;

            return accounts.Items;
        }

        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            var account =
                _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(accountName: null,
                                                                                                   accountEmail: null,
                                                                                                   confirmationToken: confirmationToken));
            return account == null ? string.Empty : account.UserName;
        }

        public MembershipCreateStatus Register(IMembershipAccount account)
        {
            //???
            return MembershipCreateStatus.Success;
        }

        public void Update(IMembershipAccount account)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(string username, bool deleteAllRelatedData)
        {
            var account =
                _viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(accountName: username,
                                                                                                   accountEmail: null,
                                                                                                   confirmationToken: null));
            return true;
        }

        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            throw new System.NotImplementedException();
        }


        public void ChangePasswordQuestionAndAnswer(string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public void ChangePassword(string newPassword)
        {
            throw new NotImplementedException();
        }

        public void ResetPassword(string newPassword, string passwordSalt)
        {
            throw new NotImplementedException();
        }

        public void LockUser()
        {
            throw new NotImplementedException();
        }

        public void UnlockUser()
        {
            throw new NotImplementedException();
        }

        public void UserValidated()
        {
            throw new NotImplementedException();
        }

        public void UpdateOnlineState()
        {
            throw new NotImplementedException();
        }

        public void ConfirmAccount()
        {
            throw new NotImplementedException();
        }
    }
}
