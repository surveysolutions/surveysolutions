using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupervisorLoginService : ISupervisorLoginService
    {
        private readonly IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins;
        private readonly IQueryableReadSideRepositoryReader<SupervisorCredentialsView> queryableReadSideRepositoryReader;
        private readonly IPasswordHasher passwordHasher;

        public SupervisorLoginService(IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins, 
            IQueryableReadSideRepositoryReader<SupervisorCredentialsView> queryableReadSideRepositoryReader, 
            IPasswordHasher passwordHasher)
        {
            this.supervisorLogins = supervisorLogins;
            this.queryableReadSideRepositoryReader = queryableReadSideRepositoryReader;
            this.passwordHasher = passwordHasher;
        }

        public bool IsUnique(string login)
        {
            return supervisorLogins.GetById(login) == null;
        }

        public bool AreCredentialsValid(string login, string password)
        {
            var hash = passwordHasher.Hash(password);
            return queryableReadSideRepositoryReader.GetById(string.Join(":", login, hash)) != null;
        }
    }
}