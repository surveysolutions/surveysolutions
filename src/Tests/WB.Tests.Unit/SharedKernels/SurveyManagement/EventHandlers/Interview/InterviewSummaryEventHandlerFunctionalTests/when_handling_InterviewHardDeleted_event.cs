using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewHardDeleted_event : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer(userId: responsibleId, userName: responsibleName);
        };

        Because of = () =>
            updatedModel = denormalizer.Update(viewModel, Create.InterviewHardDeletedEvent(userId: responsibleId));

        It should_updatedModel_be_marked_as_deleted = () =>
            updatedModel.IsDeleted.ShouldEqual(true);

        static InterviewSummaryEventHandlerFunctional denormalizer;
        static InterviewSummary viewModel;
        static InterviewSummary updatedModel;
        static string responsibleId = "11111111111111111111111111111111";
        static string responsibleName = "responsible";
    }
}
