using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_deleted : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_delete_interview()
        {
            var viewModel = new InterviewSummary();
            var denormalizer = CreateDenormalizer();

            // Act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Deleted));

            // Assert
            Assert.That(updatedModel, Is.Null);
        }
    }
}

