using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerPrincipal : IInterviewerPrincipal
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public bool IsAuthenticated => this.currentUserIdentity != null;

        private InterviewerIdentity currentUserIdentity;
        public IInterviewerUserIdentity CurrentUserIdentity => this.currentUserIdentity;
        IUserIdentity IPrincipal.CurrentUserIdentity => this.currentUserIdentity;

        public InterviewerPrincipal(IPlainStorage<InterviewerIdentity> interviewersPlainStorage)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            var localInterviewers = this.interviewersPlainStorage
                .Where(interviewer => interviewer.Password == password); // db query

            var localInterviewer = localInterviewers // memory query
                .FirstOrDefault(interviewer => string.Equals(interviewer.Name, userName, StringComparison.OrdinalIgnoreCase));

            this.currentUserIdentity = localInterviewer;

            return this.IsAuthenticated;
        }

        public void SignOut()=> this.currentUserIdentity = null;
    }
}