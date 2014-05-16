using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewOnClientCreated_event_and_responsible_user_not_found : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            denormalizer = CreateDenormalizer();
        };

        Because of = () => 
            viewModel = denormalizer.Create(Create.InterviewOnClientCreatedEvent());

        It should_commented_statuses_history_first_item_responsible_be_equal_to_unknownResponsibleName = () =>
            viewModel.CommentedStatusesHistory.Single().Responsible.ShouldEqual(unknownResponsibleName);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static string unknownResponsibleName = "<UNKNOWN RESPONSIBLE>";
    }
}
