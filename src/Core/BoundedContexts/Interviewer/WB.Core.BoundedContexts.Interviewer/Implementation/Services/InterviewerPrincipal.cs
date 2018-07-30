using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : EnumeratorPrincipal, IInterviewerPrincipal
    {
        private readonly IPlainStorage<InterviewerIdentity> usersStorage;

        public InterviewerPrincipal(IPlainStorage<InterviewerIdentity> usersStorage,
            IPasswordHasher passwordHasher) : base(passwordHasher)
        {
            this.usersStorage = usersStorage;
        }

        protected override IUserIdentity GetUserById(string userId)
            => this.usersStorage.GetById(userId);

        public IInterviewerUserIdentity CurrentUserIdentity => (IInterviewerUserIdentity)base.currentUserIdentity;

        protected override void UpdateUserHash(string hash)
        {
            var user = this.usersStorage.Where(u => u.Name.ToLower() == CurrentUserIdentity.Name).FirstOrDefault();
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
