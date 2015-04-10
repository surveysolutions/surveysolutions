using System;
using Machine.Specifications;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusChangeHistoryTests
{
    internal class when_interview_deleted_and_restored_by_synchronization : InterviewStatusChangeHistoryDenormalizerTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewStatusHistory("interviewId");

            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
        {
            viewModel = denormalizer.Update(viewModel,
                Create.InterviewDeletedEvent(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
            viewModel = denormalizer.Update(viewModel,
                Create.InterviewRestoredEvent(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
        };

        It should_not_add_sync_related_status_to_history = () => viewModel.StatusChangeHistory.ShouldBeEmpty();

        static InterviewStatusHistory viewModel;
        static InterviewStatusChangeHistoryDenormalizer denormalizer;
        static string responsibleId;
    }
}