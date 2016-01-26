using System;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : IInterviewerPrincipal
    {
        private const string UserNameParameterName = "authenticatedUser";
        
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public bool IsAuthenticated => this.currentUserIdentity != null;

        private InterviewerIdentity currentUserIdentity;
        public IInterviewerUserIdentity CurrentUserIdentity => this.currentUserIdentity;
        IUserIdentity IPrincipal.CurrentUserIdentity => this.currentUserIdentity;

        public InterviewerPrincipal(IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            var localInterviewer = this.interviewersPlainStorage
                .Where(interviewer => interviewer.Password == password) // db query
                .Where(interviewer => string.Equals(interviewer.Name, userName, StringComparison.OrdinalIgnoreCase)) // memory query
                .FirstOrDefault();
            
            this.currentUserIdentity = localInterviewer;

            return this.IsAuthenticated;
        }

        public void SignOut()
        {
            this.currentUserIdentity = null;
        }
    }
}