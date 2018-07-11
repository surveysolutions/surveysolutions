using Moq;
using MvvmCross.Tests;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class LoginViewModelTestContext : MvxIoCSupportingTest
    {
        public LoginViewModelTestContext()
        {
            base.Setup();
        }
        
        protected static LoginViewModel CreateLoginViewModel(
            IViewModelNavigationService viewModelNavigationService = null,
            IPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IRemoteAuthorizationService synchronizationService = null,
            ILogger logger = null)
        {
            return new LoginViewModel(
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                principal ?? Mock.Of<IPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                new InMemoryPlainStorage<CompanyLogo>(), 
                synchronizationService ?? Mock.Of<IRemoteAuthorizationService>(),
                logger ?? Mock.Of<ILogger>(),
                Mock.Of<IAuditLogService>());
        }
        
        protected static InterviewerIdentity CreateInterviewerIdentity(string userName, string userPasswordHash = null, string token = null)
        {
            return new InterviewerIdentity { Name = userName, PasswordHash = userPasswordHash , Token = token};
        }
    }
}
