using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowAdded_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterScopes = CreateQuestionnaireRosterScopes(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterScopes);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new RosterInstancesAdded(new[] { new AddedRosterInstance(rosterGroupId, new decimal[0], 0.0m, null) })));

        It should_interview_levels_count_be_equal_to_2 = () =>
            viewState.Levels.Keys.Count.ShouldEqual(2);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Levels.Keys.ShouldContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
