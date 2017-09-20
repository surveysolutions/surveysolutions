using Machine.Specifications;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [TestOf(typeof(InterviewSummaryDenormalizer))]
    internal class InterviewSummaryDenormalizerTests : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void when_interview_status_changed_to_completed()
        {
            //arrange
            var viewModel = new InterviewSummary();
            var denormalizer = CreateDenormalizer();

            //act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Completed));

            //assert
            updatedModel.Status.ShouldEqual(InterviewStatus.Completed);
            updatedModel.WasCompleted.ShouldBeTrue();
        }

        [Test]
        public void when_interview_was_completed_and_then_reassigned_to_supervisor()
        {
            //arrange
            var viewModel = new InterviewSummary {WasCompleted = true};
            var denormalizer = CreateDenormalizer();

            //act
            var updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.SupervisorAssigned));

            //assert
            updatedModel.Status.ShouldEqual(InterviewStatus.SupervisorAssigned);
            updatedModel.WasCompleted.ShouldBeTrue();
        }
    }
}