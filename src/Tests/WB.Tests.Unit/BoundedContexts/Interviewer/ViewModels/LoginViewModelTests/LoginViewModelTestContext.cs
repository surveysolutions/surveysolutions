using System;
using System.Linq;
using MvvmCross.Test.Core;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

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
            ISynchronizationService synchronizationService = null,
            ILogger logger = null)
        {
            return new LoginViewModel(
                viewModelNavigationService ?? Mock.Of<IViewModelNavigationService>(),
                principal ?? Mock.Of<IPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger ?? Mock.Of<ILogger>());
        }
        
        protected static InterviewerIdentity CreateInterviewerIdentity(string userName, string userPasswordHash = null)
        {
            return new InterviewerIdentity { Name = userName, Password = userPasswordHash };
        }
    }
}
