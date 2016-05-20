using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_received_by_interviewer : InterviewSummaryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            viewModel = Create.Other.InterviewSummary();
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.Event.InterviewReceivedByInterviewer().ToPublishedEvent());

        It should_mark_summary_as_received_by_interviewer = () => updatedModel.ReceivedByInterviewer.ShouldBeTrue();

        static InterviewSummary viewModel;
        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary updatedModel;
    }
}