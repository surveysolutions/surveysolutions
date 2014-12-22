﻿using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_InterviewRejected_event_and_responsible_user_not_found : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary();

            denormalizer = CreateDenormalizer();
        };

        Because of = () => 
            viewModel = denormalizer.Update(viewModel, Create.InterviewRejectedEvent());

        It should_commented_statuses_history_first_item_responsible_be_equal_to_unknownResponsibleName = () =>
            viewModel.CommentedStatusesHistory.Single().Responsible.ShouldEqual(unknownResponsibleName);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static string unknownResponsibleName = "<UNKNOWN RESPONSIBLE>";
    }
}
