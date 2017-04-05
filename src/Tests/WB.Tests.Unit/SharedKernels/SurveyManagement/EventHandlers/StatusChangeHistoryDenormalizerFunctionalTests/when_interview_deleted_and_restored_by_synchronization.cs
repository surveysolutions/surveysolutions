using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{

    internal class when_interview_deleted_and_restored_by_synchronization : StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewStatuses();

            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").FormatGuid();
            denormalizerFunctional = CreateDenormalizer();
        };

        Because of = () =>
        {
            viewModel = denormalizerFunctional.Update(viewModel,
                Create.PublishedEvent.InterviewDeleted(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
            viewModel = denormalizerFunctional.Update(viewModel,
                Create.PublishedEvent.InterviewRestored(userId: responsibleId, origin: Constants.HeadquartersSynchronizationOrigin));
        };

        It should_not_add_sync_related_status_to_history = () => viewModel.InterviewCommentedStatuses.ShouldBeEmpty();

        static InterviewStatuses viewModel;
        static StatusChangeHistoryDenormalizerFunctional denormalizerFunctional;
        static string responsibleId;
    }
}
