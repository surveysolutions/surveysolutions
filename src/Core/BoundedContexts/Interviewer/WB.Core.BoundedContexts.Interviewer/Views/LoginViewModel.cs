using System;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : EnumeratorLoginViewModel
    {
        private readonly IInterviewerPrincipal interviewerPrincipal;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IInterviewerPrincipal principal,
            IPasswordHasher passwordHasher,
            ICompanyLogoStorage logoStorage,
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.interviewerPrincipal = principal;
        }

        public override bool HasUser() => this.interviewerPrincipal.DoesIdentityExist();

        public override string? GetUserName()
            => this.interviewerPrincipal.GetExistingIdentityNameOrNull();

        public override string? GetUserLastWorkspace() 
            => this.interviewerPrincipal.GetLastWorkspaceOrNull();
        

        public override void UpdateLocalUser(string userName, string token, string passwordHash)
        {
            var localInterviewer = this.interviewerPrincipal.GetInterviewerByName(userName);
            if (localInterviewer == null)
                throw new NullReferenceException($"Interviewer with {userName} not found");
            localInterviewer.Token = token;
            localInterviewer.PasswordHash = passwordHash;

            this.interviewerPrincipal.SaveInterviewer(localInterviewer);
        }
    }
}
