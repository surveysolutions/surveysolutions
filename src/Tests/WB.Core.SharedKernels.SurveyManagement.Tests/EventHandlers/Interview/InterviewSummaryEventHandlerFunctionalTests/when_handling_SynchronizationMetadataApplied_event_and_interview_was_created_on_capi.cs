using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_SynchronizationMetadataApplied_event_and_interview_was_created_on_capi : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary()
            {
                WasCreatedOnClient = true,
                ResponsibleId = responsibleId,
                ResponsibleName = responsibleName
            };

            denormalizer = CreateDenormalizer();
        };

        Because of = () =>
            viewModel =
                denormalizer.Update(viewModel,
                    Create.SynchronizationMetadataAppliedEvent(status: interviewStatus));

        It should_teamLeadId_be_equal_to_specified_responsibleId = () =>
            viewModel.TeamLeadId.ShouldEqual(responsibleId);

        It should_teamLeadName_be_equal_to_specified_responsibleName = () =>
            viewModel.TeamLeadName.ShouldEqual(responsibleName);

        It should_status_be_equal_specified_interviewStatus = () =>
            viewModel.Status.ShouldEqual(interviewStatus);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static string responsibleName = "responsible";
        private static InterviewStatus interviewStatus = InterviewStatus.ApprovedByHeadquarters;
    }
}
