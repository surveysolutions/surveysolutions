using Machine.Specifications;
using Nito.AsyncEx.Synchronous;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.InterviewDashboardItemViewModelTests
{
    public class when_rejected_interview_is_created_on_client : InterviewDashboardItemViewModelTestsContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);

            var statefulInterviewRepository = Substitute.For<IStatefulInterviewRepository>();
            statefulInterviewRepository.Get(null).ReturnsForAnyArgs(interview);

            navigationServiceMock = Substitute.For<IViewModelNavigationService>();
            viewModel = GetViewModel(viewModelNavigationService: navigationServiceMock,
                interviewRepository: statefulInterviewRepository);
        };

        Because of = () => viewModel.LoadInterview().WaitAndUnwrapException();

        It should_open_prefilled_questions_section = () => navigationServiceMock.ReceivedWithAnyArgs().NavigateToPrefilledQuestionsAsync(null);

        static InterviewDashboardItemViewModel viewModel;
        static IViewModelNavigationService navigationServiceMock;
    }
}

