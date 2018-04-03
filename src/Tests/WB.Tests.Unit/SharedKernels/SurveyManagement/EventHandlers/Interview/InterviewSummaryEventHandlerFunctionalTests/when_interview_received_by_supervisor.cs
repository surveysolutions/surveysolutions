using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_received_by_supervisor : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_mark_summary_as_received_by_interviewer ()
        {
            var viewModel = Create.Entity.InterviewSummary();
            viewModel.ReceivedByInterviewer = true;
            var denormalizer = CreateDenormalizer();

            var updatedModel = denormalizer.Update(viewModel, Create.Event.InterviewReceivedBySupervisor().ToPublishedEvent());
        
            updatedModel.ReceivedByInterviewer.Should().BeFalse();
        }
    }
}