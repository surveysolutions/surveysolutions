using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewStatusChanged_event_and_no_status_history : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewModel = denormalizer.Update(viewModel, Create.InterviewStatusChangedEvent(InterviewStatus.Created));

        It should_leave_statuses_history_intact = () =>
            viewModel.CommentedStatusesHistory.Count.ShouldEqual(0);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
    }
}