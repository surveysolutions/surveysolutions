using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewHardDeleted_event : InterviewSummaryDenormalizerTestsContext
    {
        Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer(userId: responsibleId, userName: responsibleName);
        };

        Because of = () =>
            updatedModel = denormalizer.Update(viewModel, Create.PublishedEvent.InterviewHardDeleted(userId: responsibleId.FormatGuid()));

        It should_updatedModel_be_marked_as_deleted = () =>
            updatedModel.IsDeleted.ShouldEqual(true);

        static InterviewSummaryDenormalizer denormalizer;
        static InterviewSummary viewModel;
        static InterviewSummary updatedModel;
        static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        static string responsibleName = "responsible";
    }
}
