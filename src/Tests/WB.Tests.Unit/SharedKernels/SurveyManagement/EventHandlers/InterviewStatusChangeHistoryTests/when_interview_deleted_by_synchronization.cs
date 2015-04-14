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
            viewModel = new InterviewStatusHistory();

            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            denormalizerFunctional = CreateDenormalizer();
        };

        Because of = () =>
        {
            viewModel = denormalizerFunctional.Update(viewModel,
                Create.InterviewDeletedEvent(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
            viewModel = denormalizerFunctional.Update(viewModel,
                Create.InterviewRestoredEvent(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
        };

        It should_not_add_sync_related_status_to_history = () => viewModel.StatusChangeHistory.ShouldBeEmpty();

        static InterviewStatusHistory viewModel;
        static StatusChangeHistoryDenormalizerFunctional denormalizerFunctional;
        static string responsibleId;
    }
}