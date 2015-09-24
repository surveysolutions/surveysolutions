using System;
using System.Linq;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Interviewer.Infrastructure.Internals.Security
{
    public class InterviewerPrincipal : IInterviewerPrincipal
    {
        private const string UserNameParameterName = "authenticatedUser";

        private readonly ISettings settingsService;
        private readonly IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public bool IsAuthenticated { get { return this.currentUserIdentity != null; } }

        private InterviewerIdentity currentUserIdentity;
        public IInterviewerUserIdentity CurrentUserIdentity { get { return this.currentUserIdentity; } }
        IUserIdentity IPrincipal.CurrentUserIdentity { get { return this.currentUserIdentity; } }

        public InterviewerPrincipal(ISettings settingsService, IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage)
        {
            this.settingsService = settingsService;
            this.interviewersPlainStorage = interviewersPlainStorage;

            this.InitializeIdentity();
        }

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            var localInterviewer = this.interviewersPlainStorage.Query(
                query => query.FirstOrDefault(interviewer => string.Equals(interviewer.Name, userName, StringComparison.OrdinalIgnoreCase) 
                    && interviewer.Password == password));

            if (localInterviewer == null) return false;

            this.settingsService.AddOrUpdateValue(UserNameParameterName, userName);
            this.currentUserIdentity = localInterviewer;

            return true;
        }

        public void SignOut()
        {
            this.settingsService.DeleteValue(UserNameParameterName);
            this.currentUserIdentity = null;
        }

        private void InitializeIdentity()
        {
            var userName = this.settingsService.GetValue(UserNameParameterName, string.Empty);
            if (!string.IsNullOrEmpty(userName))
                this.currentUserIdentity = this.interviewersPlainStorage.Query(query => query.FirstOrDefault());
        }
    }
}