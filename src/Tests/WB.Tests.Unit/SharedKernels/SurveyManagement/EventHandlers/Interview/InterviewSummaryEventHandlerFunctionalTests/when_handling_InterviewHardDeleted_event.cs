using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewHardDeleted_event : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_delete_interview()
        {
            var viewModel = new InterviewSummary();

            var denormalizer = CreateDenormalizer();

            // Act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewHardDeleted());

            // Assert
            Assert.That(updatedModel, Is.Null);
        }
    }
}
