using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewerAssigned_event_and_responsible_user_not_found : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer(userId: interviewerId, userName: "interviewer name");
        };

        Because of = () =>
            viewModel =
                denormalizer.Update(viewModel,
                    Create.InterviewerAssignedEvent(interviewerId: interviewerId));

        It should_commented_statuses_history_first_item_responsible_be_equal_to_responsibleName = () =>
            viewModel.CommentedStatusesHistory.Single().Responsible.ShouldEqual(unknownResponsibleName);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static string interviewerId = "22222222222222222222222222222222";
        private static string unknownResponsibleName = "<UNKNOWN RESPONSIBLE>";
    }
}
