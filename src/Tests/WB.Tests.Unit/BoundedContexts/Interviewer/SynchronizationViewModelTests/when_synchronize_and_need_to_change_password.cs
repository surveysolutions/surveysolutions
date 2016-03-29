using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.SynchronizationViewModelTests
{
    internal class when_synchronize_and_need_to_change_password : SynchronizationViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewerIdentity = new InterviewerIdentity() {Name = "name", Password = "pass"};

            principalMock
                .Setup(x => x.CurrentUserIdentity)
                .Returns(interviewerIdentity);

            userInteractionServiceMock
                .Setup(x => x.ConfirmWithTextInputAsync(Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()))
                .Returns(Task.FromResult("new password"));

            
            synchronizationServiceMock
                .Setup(x => x.CanSynchronizeAsync(Moq.It.Is<RestCredentials>(r=>r.Password== interviewerIdentity.Password), Moq.It.IsAny<CancellationToken>()))
                .Throws(new SynchronizationException(type: SynchronizationExceptionType.Unauthorized, message: "", innerException: null));
            
            passwordHasherMock
                .Setup(x => x.Hash(Moq.It.IsAny<string>()))
                .Returns<string>(x => x);

            interviewerStorageMock
                .Setup(x => x.FirstOrDefault())
                .Returns(interviewerIdentity);

            viewModel = Create.SynchronizationViewModel(principal: principalMock.Object,
                synchronizationService: synchronizationServiceMock.Object,
                interviewersPlainStorage: interviewerStorageMock.Object,
                userInteractionService: userInteractionServiceMock.Object,
                passwordHasher: passwordHasherMock.Object
                );
        };

        Because of = async () => await viewModel.Synchronize();

        It should_store_updated_user_password_in_plain_storage = () =>
            interviewerStorageMock.Verify(
                x => x.StoreAsync(Moq.It.Is<InterviewerIdentity>(i => i.Password == "new password")), Times.Once);

        It should_sign_in_user_with_new_credentials = () =>
           principalMock.Verify(x => x.SignInAsync("name", "new password", true), Times.Once);

        static SynchronizationViewModel viewModel;
        static Mock<IAsyncPlainStorage<InterviewerIdentity>> interviewerStorageMock = new Mock<IAsyncPlainStorage<InterviewerIdentity>>();
        static Mock<IUserInteractionService> userInteractionServiceMock=new Mock<IUserInteractionService>();
        static Mock<IPrincipal> principalMock = new Mock<IPrincipal>();
        static Mock<ISynchronizationService>  synchronizationServiceMock =new Mock<ISynchronizationService>();
        static Mock<IPasswordHasher> passwordHasherMock = new Mock<IPasswordHasher>();
    }
}