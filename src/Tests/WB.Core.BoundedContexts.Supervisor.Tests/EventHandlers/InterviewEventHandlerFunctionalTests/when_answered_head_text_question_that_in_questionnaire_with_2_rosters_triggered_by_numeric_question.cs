using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewEventHandlerFunctionalTests
{
    [Ignore]
    internal class when_answered_head_text_question_that_in_questionnaire_with_2_rosters_triggered_by_numeric_question : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroup1Id = Guid.Parse("10000000000000000000000000000001");
            rosterGroup2Id = Guid.Parse("20000000000000000000000000000002");

            rosterScopeId = Guid.Parse("12222222222222222222222222222222");

            headQuestionId = Guid.Parse("13333333333333333333333333333333");

            viewState = CreateViewWithSequenceOfInterviewData();

            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, Guid?>() { { rosterGroup1Id, headQuestionId }, { rosterGroup2Id, headQuestionId } });

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState, CreatePublishableEvent(new RosterRowAdded(rosterGroup1Id, new decimal[0], 0, null)));
            viewState = interviewEventHandlerFunctional.Update(viewState, CreatePublishableEvent(new RosterRowAdded(rosterGroup2Id, new decimal[0], 0, null)));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new TextQuestionAnswered(Guid.NewGuid(), headQuestionId, new decimal[] { 0 }, DateTime.Now, helloRoster)));

        It should_roster_title_be_equal_to_recived_answer_for_the_first_roster = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroup1Id].ShouldEqual(helloRoster);

        It should_roster_title_be_equal_to_recived_answer_for_the_second_roster = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroup2Id].ShouldEqual(helloRoster);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroup1Id;
        private static Guid rosterGroup2Id;
        private static Guid rosterScopeId;
        private static Guid headQuestionId;
        private static string helloRoster = "hello roster";
    }
}