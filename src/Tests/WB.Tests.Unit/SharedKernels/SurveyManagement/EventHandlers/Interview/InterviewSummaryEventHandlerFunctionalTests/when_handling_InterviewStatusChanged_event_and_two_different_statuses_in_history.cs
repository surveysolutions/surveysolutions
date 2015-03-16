using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewStatusChanged_event_and_two_different_statuses_in_history :
        InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary
            {
            };

            viewModel.CommentedStatusesHistory.Add(new InterviewCommentedStatus
            {
                Status = InterviewStatus.Completed,
                Comment = thisCommentShouldNotBeChanged
            });

            viewModel.CommentedStatusesHistory.Add(new InterviewCommentedStatus
            {
                Status = InterviewStatus.RejectedBySupervisor,
                Comment = null
            });

            denormalizer = CreateDenormalizer();
        };

        private Because of = () =>
            viewModel = denormalizer.Update(viewModel, Create.InterviewStatusChangedEvent(InterviewStatus.Completed, comment));

        private It should_leave_statuses_history_intact = () =>
            viewModel.CommentedStatusesHistory.Last().Comment.ShouldBeNull();

        private It should_set_not_change_comment_for_first_status = () =>
            viewModel.CommentedStatusesHistory.First().Comment.ShouldEqual(thisCommentShouldNotBeChanged);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private const string comment = "Some status change comment";
        private static string thisCommentShouldNotBeChanged = "Should not be changed";
    }
}