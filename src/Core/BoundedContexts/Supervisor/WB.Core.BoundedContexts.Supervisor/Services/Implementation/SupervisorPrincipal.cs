using System;
using System.Linq;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
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

        protected override void UpdateUserHash(string userId, string hash)
        {
            var user = this.usersStorage.GetById(userId);
            if (user != null)
            {
                user.PasswordHash = hash;
                this.usersStorage.Store(user);
            }
        }

        protected override IUserIdentity GetUserByName(string userName)
                => this.usersStorage.Where(user => user.Name.ToLower() == userName).FirstOrDefault();
    }
}
