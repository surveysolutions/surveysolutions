using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_status_changed : InterviewSummaryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            viewModel = new InterviewSummary();
            viewModel.WasRejectedBySupervisor = true;
            denormalizer = CreateDenormalizer();
        };

        Because of = () => updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

        It should_change_interview_status = () => updatedModel.Status.ShouldEqual(InterviewStatus.InterviewerAssigned);

        It should_not_change_WasRejectedBySupervisor_flag = () => updatedModel.WasRejectedBySupervisor.ShouldBeTrue();

        static InterviewSummary viewModel;
        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary updatedModel;
    }
}

