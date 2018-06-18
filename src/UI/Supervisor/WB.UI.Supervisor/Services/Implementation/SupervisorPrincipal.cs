using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Supervisor.Services.Implementation
{
    public class SupervisorPrincipal : EnumeratorPrincipal, ISupervisorPrincipal
    {
        private readonly IPlainStorage<SupervisorIdentity> usersStorage;

        public SupervisorPrincipal(IPlainStorage<SupervisorIdentity> usersStorage,
            IPasswordHasher passwordHasher) : base(passwordHasher)
        {
            this.usersStorage = usersStorage;
        }

        protected override IUserIdentity GetUserById(string userId)
            => this.usersStorage.GetById(userId);

        public ISupervisorUserIdentity CurrentUserIdentity => (ISupervisorUserIdentity)base.currentUserIdentity;

        protected override IUserIdentity GetUserByName(string userName)
            => this.usersStorage.Where(user => user.Name.ToLower() == userName).FirstOrDefault();
    }
}
