﻿using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowRemoved_event_recived_for_disabled_group : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            disabledGroupId = Guid.Parse("20000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            var someAdditionalScopeId = Guid.Parse("22222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();

            var interviewLevel = new InterviewLevel(new ValueVector<Guid> { rosterScopeId }, null, new decimal[0]);
            interviewLevel.DisabledGroups.Add(disabledGroupId);
            interviewLevel.ScopeVectors.Add(new ValueVector<Guid> { someAdditionalScopeId }, null);

            viewState.Levels.Add("0", interviewLevel);

            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.FixedRoster(rosterGroupId,
                    fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")}),

                Create.Entity.FixedRoster(disabledGroupId,
                    fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")})
            });

            var questionnaireRosterScopes = CreateQuestionnaireRosterScopes(rosterScopeId, rosterGroupId, disabledGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterScopes, questionnaireDocument : questionnaire);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(Create.Event.RosterInstancesRemoved(disabledGroupId)));

        It should_interview_levels_count_be_equal_to_2_top_level_and_roster_row = () =>
            viewState.Levels.Keys.Count.ShouldEqual(2);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Levels.Keys.ShouldContain("0");

        It should_interview_level_with_id_0_do_not_disabledGroupId = () =>
           viewState.Levels["0"].DisabledGroups.ShouldNotContain(disabledGroupId);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid disabledGroupId;
        private static Guid rosterScopeId;
    }
}
