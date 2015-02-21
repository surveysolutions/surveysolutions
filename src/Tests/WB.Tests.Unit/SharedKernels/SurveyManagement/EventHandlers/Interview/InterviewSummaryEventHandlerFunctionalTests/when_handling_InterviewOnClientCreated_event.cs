using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewOnClientCreated_event : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            denormalizer = CreateDenormalizer(userId: responsibleId, userName: responsibleName);
        };

        Because of = () =>
            viewModel = denormalizer.Update(null, Create.InterviewOnClientCreatedEvent(userId: responsibleId));

        It should_commented_statuses_history_not_be_empty = () =>
            viewModel.CommentedStatusesHistory.ShouldNotBeEmpty();

        It should_commented_statuses_history_has_one_item = () =>
            viewModel.CommentedStatusesHistory.Count.ShouldEqual(1);

        It should_commented_statuses_history_first_item_status_be_equal_to_interview_status_created = () =>
            viewModel.CommentedStatusesHistory.Single().Status.ShouldEqual(InterviewStatus.Created);

        It should_commented_statuses_history_first_item_responsible_be_equal_to_responsibleName = () =>
            viewModel.CommentedStatusesHistory.Single().Responsible.ShouldEqual(responsibleName);

        It should_commented_statuses_history_first_item_comment_be_null = () =>
            viewModel.CommentedStatusesHistory.Single().Comment.ShouldBeNull();

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static string responsibleId = "11111111111111111111111111111111";
        private static string responsibleName = "responsible";
    }
}
