using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_deleted : InterviewSummaryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            viewModel = new InterviewSummary();
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Deleted));

        It should_mark_summary_as_deleted = () => updatedModel.IsDeleted.ShouldBeTrue();

        It should_change_summary_status = () => updatedModel.Status.ShouldEqual(InterviewStatus.Deleted);

        static InterviewSummary viewModel;
        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary updatedModel;
    }
}

