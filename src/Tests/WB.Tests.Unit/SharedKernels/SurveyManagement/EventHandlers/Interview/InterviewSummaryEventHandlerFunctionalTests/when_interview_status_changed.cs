using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_status_changed : InterviewSummaryDenormalizerTestsContext
    {
        [Test] 
        public void should_change_interview_status_and_should_not_change_WasRejectedBySupervisor_flag()
        {
            var viewModel = new InterviewSummary();
            viewModel.WasRejectedBySupervisor = true;
            var denormalizer = CreateDenormalizer();

            // Act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

            // Assert
            updatedModel.Status.Should().Be(InterviewStatus.InterviewerAssigned);
            updatedModel.WasRejectedBySupervisor.Should().BeTrue();
        }
    }
}

