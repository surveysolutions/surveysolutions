using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_received_by_supervisor : InterviewSummaryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            viewModel = Create.Entity.InterviewSummary();
            viewModel.ReceivedByInterviewer = true;
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.Event.InterviewReceivedBySupervisor().ToPublishedEvent());

        It should_mark_summary_as_received_by_interviewer = () => updatedModel.ReceivedByInterviewer.ShouldBeFalse();

        static InterviewSummary viewModel;
        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary updatedModel;
    }
}