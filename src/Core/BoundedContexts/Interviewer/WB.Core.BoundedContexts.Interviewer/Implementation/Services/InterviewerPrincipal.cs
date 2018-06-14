using System;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : EnumeratorPrincipal, IInterviewerPrincipal
    {
        private readonly IPlainStorage<InterviewerIdentity> usersStorage;
        [Obsolete("Should be removed if there is no v5.18 in the wild")]
        private static readonly Lazy<PasswordHasher> OldPasswordHasher = new Lazy<PasswordHasher>(() => new PasswordHasher());

        public InterviewerPrincipal(IPlainStorage<InterviewerIdentity> usersStorage,
            IPasswordHasher passwordHasher) : base(passwordHasher)
        {
            this.usersStorage = usersStorage;
        }

        protected override IUserIdentity GetUserById(string userId)
            => this.usersStorage.GetById(userId);

        public IInterviewerUserIdentity CurrentUserIdentity => (IInterviewerUserIdentity)base.currentUserIdentity;

        protected override IUserIdentity GetUserByName(string userName)
            => this.usersStorage.Where(user => user.Name.ToLower() == userName).FirstOrDefault();

        protected override void UpdatePasswordHash(IUserIdentity localUser, string password)
        {
            var localInterviewer = (InterviewerIdentity) localUser;

            if (localInterviewer.Password == null ||
                !OldPasswordHasher.Value.VerifyPassword(localInterviewer.Password, password)) return;

            localInterviewer.PasswordHash = this.passwordHasher.Hash(password);
            localInterviewer.Password = null;
            this.usersStorage.Store(localInterviewer);
        }
    }
}
