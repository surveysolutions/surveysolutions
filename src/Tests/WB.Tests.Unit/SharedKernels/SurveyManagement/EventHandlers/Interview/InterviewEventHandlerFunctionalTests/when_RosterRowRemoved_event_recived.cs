﻿using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowRemoved_event_recived : InterviewEventHandlerFunctionalTestContext
    {
        private Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.FixedRoster(rosterGroupId,
                    fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")})
            });

            viewState = CreateViewWithSequenceOfInterviewData();
            viewState.Levels.Add("0", new InterviewLevel(new ValueVector<Guid> { rosterScopeId }, null, new decimal[0]));

            var questionnaireRosterScopes = CreateQuestionnaireRosterScopes(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterScopes,questionnaireDocument : questionnaire);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(Create.Event.RosterInstancesRemoved(rosterGroupId)));

        It should_interview_levels_count_be_equal_to_1 = () =>
            viewState.Levels.Keys.Count.ShouldEqual(1);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Levels.Keys.ShouldNotContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
