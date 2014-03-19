using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class SupervisorLoginService : ISupervisorLoginService
    {
        private readonly IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins;

        public SupervisorLoginService(IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins)
        {
            this.supervisorLogins = supervisorLogins;
        }

        public bool IsUnique(string login)
        {
            return supervisorLogins.GetById(login) == null;
        }

        public bool AreCredentialsValid(string login, string password)
        {
            throw new System.NotImplementedException();
        }
    }
}