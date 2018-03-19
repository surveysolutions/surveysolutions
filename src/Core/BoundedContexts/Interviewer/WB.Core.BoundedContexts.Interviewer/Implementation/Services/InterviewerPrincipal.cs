using System;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : IInterviewerPrincipal
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPasswordHasher passwordHasher;

        public bool IsAuthenticated => this.currentUserIdentity != null;

        private InterviewerIdentity currentUserIdentity;
        public IInterviewerUserIdentity CurrentUserIdentity => this.currentUserIdentity;
        IUserIdentity IPrincipal.CurrentUserIdentity => this.currentUserIdentity;

        [Obsolete("Should be removed if there is no v5.18 in the wild")]
        private static readonly Lazy<PasswordHasher> OldPasswordHasher = new Lazy<PasswordHasher>(() => new PasswordHasher());

        public InterviewerPrincipal(IPlainStorage<InterviewerIdentity> interviewersPlainStorage, IPasswordHasher passwordHasher)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
        }

        private InterviewerIdentity FindIdentityByUsername(string userName)
        {
            if (userName == null) return null;

            var localInterviewer = this.interviewersPlainStorage
              .Where(interviewer => interviewer.Name.ToLower() == userName.ToLower()).FirstOrDefault(); // db query

            return localInterviewer;
        }

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            this.currentUserIdentity = null;
            
            var localInterviewer = FindIdentityByUsername(userName); // db query

            if (localInterviewer == null) return false;

            if (localInterviewer.Password != null && OldPasswordHasher.Value.VerifyPassword(localInterviewer.Password, password))
            {
                localInterviewer.PasswordHash = this.passwordHasher.Hash(password);
                localInterviewer.Password = null;
                this.interviewersPlainStorage.Store(localInterviewer);
            }

            if (localInterviewer.PasswordHash != null && this.passwordHasher.VerifyPassword(localInterviewer.PasswordHash, password))
            {
                this.currentUserIdentity = localInterviewer;
            }

            return this.IsAuthenticated;
        }

        public bool SignInWithHash(string userName, string passwordHash, bool staySignedIn)
        {
            this.currentUserIdentity = null;

            var localInterviewer = FindIdentityByUsername(userName); // db query
            
            if (string.Equals(localInterviewer.Password 
                ?? localInterviewer.PasswordHash, passwordHash, StringComparison.Ordinal))
            {
                this.currentUserIdentity = localInterviewer;
            }

            return this.IsAuthenticated;
        }

        public void SignOut() => this.currentUserIdentity = null;

        public bool SignIn(string userId, bool staySignedIn)
        {
            var interviewer = this.interviewersPlainStorage.GetById(userId);
            this.currentUserIdentity = interviewer;
            return this.IsAuthenticated;
        }
    }
}
