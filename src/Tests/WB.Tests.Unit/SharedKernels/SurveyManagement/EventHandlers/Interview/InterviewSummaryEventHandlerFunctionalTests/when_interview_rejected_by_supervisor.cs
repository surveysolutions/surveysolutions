using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_rejected_by_supervisor : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();
            viewModel.WasRejectedBySupervisor = false;
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.InterviewStatusChangedEvent(InterviewStatus.RejectedBySupervisor));

        It should_mark_summary_rejected_by_supervisor = () => updatedModel.WasRejectedBySupervisor.ShouldBeTrue();

        static InterviewSummary viewModel;
        static InterviewSummaryEventHandlerFunctional denormalizer;
        static InterviewSummary updatedModel;
    }
}

