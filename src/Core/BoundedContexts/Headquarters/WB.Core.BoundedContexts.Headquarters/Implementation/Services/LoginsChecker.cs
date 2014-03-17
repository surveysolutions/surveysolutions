using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class LoginsChecker : ILoginsChecker
    {
        private readonly IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins;

        public LoginsChecker(IQueryableReadSideRepositoryReader<SupervisorLoginView> supervisorLogins)
        {
            this.supervisorLogins = supervisorLogins;
        }

        public bool IsUnique(string login)
        {
            return supervisorLogins.GetById(login) == null;
        }
    }
}