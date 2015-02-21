using System;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_interview_restored_by_synchronization : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();

            const string responsibleName = "test";
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            denormalizer = CreateDenormalizer(userId: responsibleId, userName: responsibleName);
        };

        Because of = () =>
            viewModel = denormalizer.Update(viewModel, Create.InterviewRestoredEvent(userId: responsibleId, origin: Constants.SupervisorSynchronizationOrigin));

        It should_not_add_sync_related_status_to_history = () => viewModel.CommentedStatusesHistory.ShouldBeEmpty();

        static InterviewSummary viewModel;
        static InterviewSummaryEventHandlerFunctional denormalizer;
        static string responsibleId;
    }
}

