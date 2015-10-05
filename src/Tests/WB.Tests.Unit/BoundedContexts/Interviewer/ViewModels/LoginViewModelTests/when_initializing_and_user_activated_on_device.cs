using System;
using System.Linq;

using Machine.Specifications;

using Moq;

using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_user_activated_on_device : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);

            var interviewer = CreateInterviewerIdentity(userName);

            InterviewersPlainStorage
               .Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewerIdentity>, InterviewerIdentity>>()))
               .Returns(interviewer);

            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                interviewersPlainStorage: InterviewersPlainStorage.Object);
        };

        Because of = () => viewModel.Init();

        It should_fill_user_name = () =>
            viewModel.UserName.ShouldEqual(userName);

        It should_set_IsUserValid_in_true = () =>
            viewModel.IsUserValid.ShouldBeTrue();

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IAsyncPlainStorage<InterviewerIdentity>> InterviewersPlainStorage = new Mock<IAsyncPlainStorage<InterviewerIdentity>>();
    }
}