using Designer.Web.Providers.Membership;
using Main.Core.View;
using Ncqrs.Commanding.ServiceModel;
using System.Web.Security;

namespace Designer.Web.Providers.Repositories.CQRS
{
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

        public MembershipCreateStatus Register(IMembershipAccount account)
        {
            //???
            return MembershipCreateStatus.Success;
        }

        public IMembershipAccount Get(string username)
        {
            throw new System.NotImplementedException();
        }

        public void Update(IMembershipAccount account)
        {
            throw new System.NotImplementedException();
        }

        public IMembershipAccount GetByProviderKey(object id)
        {
            throw new System.NotImplementedException();
        }

        public string GetUserNameByEmail(string email)
        {
            throw new System.NotImplementedException();
        }

        public bool Delete(string username, bool deleteAllRelatedData)
        {
            throw new System.NotImplementedException();
        }

        public int GetNumberOfUsersOnline()
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IMembershipAccount> FindAll(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IMembershipAccount> FindNewAccounts(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IMembershipAccount> FindByUserName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public System.Collections.Generic.IEnumerable<IMembershipAccount> FindByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new System.NotImplementedException();
        }

        public IMembershipAccount Create(object providerUserKey, string applicationName, string username, string email)
        {
            throw new System.NotImplementedException();
        }


        public string GetUserNameByConfirmationToken(string confirmationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
