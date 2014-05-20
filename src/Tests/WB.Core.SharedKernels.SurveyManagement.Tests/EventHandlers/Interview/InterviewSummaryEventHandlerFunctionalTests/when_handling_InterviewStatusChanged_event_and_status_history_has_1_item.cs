using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewStatusChanged_event_and_status_history_has_1_item : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary
            {
                CommentedStatusesHistory = new List<InterviewCommentedStatus>
                {
                    new InterviewCommentedStatus
                    {
                        Status = InterviewStatus.Completed,
                        Comment = null
                    }
                }
            };

            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewModel = denormalizer.Update(viewModel, Create.InterviewStatusChangedEvent(InterviewStatus.Completed, comment));

        It should_leave_statuses_history_intact = () =>
            viewModel.CommentedStatusesHistory.Last().Comment.ShouldEqual(comment);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private const string comment = "Some status change comment";
    }
}