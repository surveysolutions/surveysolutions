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
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public bool IsAuthenticated => this.currentUserIdentity != null;

        private InterviewerIdentity currentUserIdentity;
        public IInterviewerUserIdentity CurrentUserIdentity => this.currentUserIdentity;
        IUserIdentity IPrincipal.CurrentUserIdentity => this.currentUserIdentity;

        public InterviewerPrincipal(IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public async Task<bool> SignInAsync(string userName, string password, bool staySignedIn)
        {
            var localInterviewers = await this.interviewersPlainStorage
                .WhereAsync(interviewer => interviewer.Password == password); // db query

            var localInterviewer = localInterviewers // memory query
                .FirstOrDefault(interviewer => string.Equals(interviewer.Name, userName, StringComparison.OrdinalIgnoreCase));

            this.currentUserIdentity = localInterviewer;

            return this.IsAuthenticated;
        }

        public Task SignOutAsync()
        {
            this.currentUserIdentity = null;
            return Task.FromResult(true);
        }
    }
}