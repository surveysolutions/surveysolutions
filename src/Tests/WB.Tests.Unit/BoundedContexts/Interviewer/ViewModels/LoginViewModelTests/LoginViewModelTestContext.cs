using Moq;
using MvvmCross.Tests;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

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
            IInterviewerPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            IOnlineSynchronizationService synchronizationService = null,
            ILogger logger = null,
            ICompanyLogoStorage logoStorage = null)
        {
            return new LoginViewModel(
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                principal ?? Mock.Of<IInterviewerPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                logoStorage ?? Mock.Of<ICompanyLogoStorage>(),
                synchronizationService ?? Mock.Of<IOnlineSynchronizationService>(),
                logger ?? Mock.Of<ILogger>(),
                Mock.Of<IAuditLogService>());
        }
        
        protected static InterviewerIdentity CreateInterviewerIdentity(string userName, string userPasswordHash = null, string token = null)
        {
            return new InterviewerIdentity { Name = userName, PasswordHash = userPasswordHash , Token = token};
        }
    }
}
