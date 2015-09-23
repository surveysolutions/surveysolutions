using System;
using System.Linq;

using Cirrious.MvvmCross.Test.Core;

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
            IPrincipal principal = null,
            IPasswordHasher passwordHasher = null,
            InterviewerIdentity interviewer = null,
            ISynchronizationService synchronizationService = null,
            ILogger logger = null)
        {
            InterviewersPlainStorage
               .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewerIdentity>, InterviewerIdentity>>()))
               .Returns(interviewer);

            return new LoginViewModel(
                ViewModelNavigationServiceMock.Object,
                principal ?? Mock.Of<IPrincipal>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>(),
                InterviewersPlainStorage.Object,
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                logger ?? Mock.Of<ILogger>());
        }

        protected readonly static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();

        protected static readonly Mock<IAsyncPlainStorage<InterviewerIdentity>> InterviewersPlainStorage = new Mock<IAsyncPlainStorage<InterviewerIdentity>>();

        protected static InterviewerIdentity CreateInterviewerIdentity(string userName, string userPasswordHash = null)
        {
            return new InterviewerIdentity { Name = userName, Password = userPasswordHash };
        }
    }
}
