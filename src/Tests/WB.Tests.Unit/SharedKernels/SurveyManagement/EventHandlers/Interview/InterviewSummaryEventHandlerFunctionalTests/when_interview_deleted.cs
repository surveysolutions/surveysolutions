using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_deleted : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        Establish context = () =>
        {
            viewModel = new InterviewSummary();
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.InterviewStatusChangedEvent(InterviewStatus.Deleted));

        It should_mark_summary_as_deleted = () => updatedModel.IsDeleted.ShouldBeTrue();

        It should_change_summary_status = () => updatedModel.Status.ShouldEqual(InterviewStatus.Deleted);

        static InterviewSummary viewModel;
        static InterviewSummaryEventHandlerFunctional denormalizer;
        static InterviewSummary updatedModel;
    }
}

