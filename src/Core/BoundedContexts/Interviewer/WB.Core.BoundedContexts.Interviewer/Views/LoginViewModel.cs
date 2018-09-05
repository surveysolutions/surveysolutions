using MvvmCross.Navigation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class LoginViewModel : EnumeratorLoginViewModel
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<CompanyLogo> logoStorage,
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public override bool HasUser() => this.interviewersPlainStorage.FirstOrDefault() != null;

        public override string GetUserName()
            => this.interviewersPlainStorage.FirstOrDefault().Name;

        public override void UpdateLocalUser(string userName, string token, string passwordHash)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.Token = token;
            localInterviewer.PasswordHash = passwordHash;

            this.interviewersPlainStorage.Store(localInterviewer);
        }
    }
}
