using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_rejected_by_supervisor : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_mark_summary_rejected_by_supervisor()
        {
            var viewModel = new InterviewSummary();
            viewModel.WasRejectedBySupervisor = false;
            var denormalizer = CreateDenormalizer();

            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.RejectedBySupervisor));

            updatedModel.WasRejectedBySupervisor.Should().BeTrue();
        }
    }
}

