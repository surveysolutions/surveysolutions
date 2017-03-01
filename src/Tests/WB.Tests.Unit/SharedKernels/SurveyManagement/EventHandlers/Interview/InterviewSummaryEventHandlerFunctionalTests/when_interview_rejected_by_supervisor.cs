using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_rejected_by_supervisor : InterviewSummaryDenormalizerTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();
            viewModel.WasRejectedBySupervisor = false;
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.RejectedBySupervisor));

        It should_mark_summary_rejected_by_supervisor = () => updatedModel.WasRejectedBySupervisor.ShouldBeTrue();

        static InterviewSummary viewModel;
        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary updatedModel;
    }
}

