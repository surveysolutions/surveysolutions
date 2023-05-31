using System;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : EnumeratorPrincipal, IInterviewerPrincipal
    {
        private readonly IPlainStorage<InterviewerIdentity> usersStorage;
        
        public InterviewerPrincipal(IPlainStorage<InterviewerIdentity> usersStorage,
            IPasswordHasher passwordHasher,
            ILogger logger) : base(passwordHasher, logger)
        {
            this.usersStorage = usersStorage;
        }

        protected override IUserIdentity GetUserById(string userId)
            => this.usersStorage.GetById(userId);

        //public IInterviewerUserIdentity CurrentUserIdentity => (IInterviewerUserIdentity)base.currentUserIdentity;
        
        private InterviewerIdentity GetInterviewerIdentity()
        {
            return this.usersStorage
                //.Where(x => x.Id != null)
                //.OrderByDescending(x => x.Name)
                .FirstOrDefault();
        }

        public bool DoesIdentityExist()
        {
            return GetInterviewerIdentity() != null;
        }

        public string? GetExistingIdentityNameOrNull()
        {
            return GetInterviewerIdentity()?.Name;
        }

        public string? GetLastWorkspaceOrNull()
        {
            return GetInterviewerIdentity()?.Workspace;
        }

        public bool SaveInterviewer(InterviewerIdentity interviewer)
        {
            var user = this.usersStorage.GetById(interviewer.Id);
            
            if(user == null)
                interviewer.Created = DateTime.Now;

            interviewer.LastUpdated = DateTime.Now;
            this.usersStorage.Store(interviewer);
            
            return user == null;
        }
        
        public InterviewerIdentity? GetInterviewerByName(string name)
        {
            var userName = name.ToLower();
            var interviewerIdentity = this.usersStorage.Where(user => user.Name.ToLower() == userName).FirstOrDefault();
            return interviewerIdentity;
        }

        protected override void UpdateUserHash(string userId, string hash)
        {
            var user = this.usersStorage.GetById(userId);

            if (user != null)
            {
                user.PasswordHash = hash;
                this.usersStorage.Store(user);
            }
        }

        protected override IUserIdentity? GetUserByName(string userName)
            => this.GetInterviewerByName(userName);
    }
}
