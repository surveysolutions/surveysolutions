using FluentAssertions;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_received_by_interviewer : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_mark_summary_as_received_by_interviewer ()
        {
            var viewModel = Create.Entity.InterviewSummary();
            var denormalizer = CreateDenormalizer();

            var updatedModel = denormalizer.Update(viewModel, Create.Event.InterviewReceivedByInterviewer().ToPublishedEvent());

            updatedModel.ReceivedByInterviewer.Should().BeTrue();
        }
    }
}