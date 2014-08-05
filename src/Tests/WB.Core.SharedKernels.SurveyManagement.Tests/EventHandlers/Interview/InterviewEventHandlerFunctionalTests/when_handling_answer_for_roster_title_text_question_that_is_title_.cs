using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_handling_answer_for_roster_title_text_question_that_is_title_for_two_rosters_triggered_by_numeric_question : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroup1Id = Guid.Parse("10000000000000000000000000000001");
            rosterGroup2Id = Guid.Parse("20000000000000000000000000000002");

            rosterScopeId = Guid.Parse("12222222222222222222222222222222");

            rosterTitleQuestionId = Guid.Parse("13333333333333333333333333333333");

            viewState = CreateViewWithSequenceOfInterviewData();

            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, Guid?>() { { rosterGroup1Id, rosterTitleQuestionId }, { rosterGroup2Id, rosterTitleQuestionId } });

            textQuestionAnsweredEvent = CreateTextQuestionAnsweredEvent(rosterTitleQuestionId, new decimal[] { 0 }, receivedAnswer);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState, CreatePublishableEvent(new RosterRowAdded(rosterGroup1Id, new decimal[0], 0, null)));
            viewState = interviewEventHandlerFunctional.Update(viewState, CreatePublishableEvent(new RosterRowAdded(rosterGroup2Id, new decimal[0], 0, null)));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(textQuestionAnsweredEvent));

        It shoult_set_first_roster_title_to_received_answer = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroup1Id].ShouldEqual(receivedAnswer);

        It shoult_set_second_roster_title_to_received_answer = () =>
            viewState.Document.Levels["0"].RosterRowTitles[rosterGroup2Id].ShouldEqual(receivedAnswer);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid rosterGroup1Id;
        private static Guid rosterGroup2Id;
        private static Guid rosterScopeId;
        private static Guid rosterTitleQuestionId;
        private static string receivedAnswer = "hello roster";
        private static TextQuestionAnswered textQuestionAnsweredEvent;
    }
}