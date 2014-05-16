using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewApproved_event : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer(userId: responsibleId, userName: responsibleName);
        };

        Because of = () => 
            viewModel = denormalizer.Update(viewModel, Create.InterviewApprovedEvent(userId: responsibleId, comment: comment));

        It should_commented_statuses_history_not_be_empty = () =>
            viewModel.CommentedStatusesHistory.ShouldNotBeEmpty();

        It should_commented_statuses_history_has_one_item = () =>
            viewModel.CommentedStatusesHistory.Count.ShouldEqual(1);

        It should_commented_statuses_history_first_item_status_be_equal_to_interview_status_approved_by_supervisor = () =>
            viewModel.CommentedStatusesHistory.Single().Status.ShouldEqual(InterviewStatus.ApprovedBySupervisor);

        It should_commented_statuses_history_first_item_responsible_be_equal_to_responsibleName = () =>
            viewModel.CommentedStatusesHistory.Single().Responsible.ShouldEqual(responsibleName);

        It should_commented_statuses_history_first_item_comment_be_null = () =>
            viewModel.CommentedStatusesHistory.Single().Comment.ShouldEqual(comment);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static string responsibleId = "11111111111111111111111111111111";
        private static string responsibleName = "responsible";
        private static string comment = "comment";
    }
}
