using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_RosterRowRemoved_event_recived_having_answered_questions : InterviewEventHandlerFunctionalTestContext
    {
        private Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");

            question1Id = Guid.Parse("90000000000000000000000000000000");
            rosterScope1Id = Guid.Parse("92222222222222222222222222222222");


            staticText1Id = Guid.Parse("82222222222222222222222222222222");

            group1Id = Guid.Parse("72222222222222222222222222222222");

            variable1Id = Guid.Parse("62222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.FixedRoster(rosterGroupId,
                    fixedTitles: new FixedRosterTitle[] {new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2")}, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(question1Id),
                        Create.Entity.StaticText(staticText1Id),
                        Create.Entity.Group(group1Id),
                        Create.Entity.Variable(variable1Id)
                    })
            });

            viewState = CreateViewWithSequenceOfInterviewData();
            viewState.Levels.Add("0", new InterviewLevel(new ValueVector<Guid> { rosterScopeId }, null, new decimal[0]));

            viewState.Levels["0"].ScopeVectors.Add(new ValueVector<Guid> { rosterScope1Id }, null);

            viewState.Levels["0"].QuestionsSearchCache.Add(question1Id, new InterviewQuestion());
            viewState.Levels["0"].StaticTexts.Add(staticText1Id, new InterviewStaticText());
            viewState.Levels["0"].DisabledGroups.Add(group1Id);
            viewState.Levels["0"].DisabledVariables.Add(variable1Id);

            var questionnaireRosterScopes = CreateQuestionnaireRosterScopes(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterScopes, questionnaireDocument : questionnaire);
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(Create.Event.RosterInstancesRemoved(rosterGroupId)));

        It should_interview_levels_count_be_equal_to_1 = () =>
            viewState.Levels.Keys.Count.ShouldEqual(2);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Levels.Keys.ShouldContain("0");

        It should_interview_level_questions_count_be_equal_to_0 = () =>
            viewState.Levels["0"].QuestionsSearchCache.Count.ShouldEqual(0);

        It should_interview_level_disabled_groups_count_be_equal_to_0 = () =>
            viewState.Levels["0"].DisabledGroups.Count.ShouldEqual(0);

        It should_interview_level_disabled_variables_count_be_equal_to_0 = () =>
            viewState.Levels["0"].DisabledVariables.Count.ShouldEqual(0);

        It should_interview_level_static_texts_count_be_equal_to_0 = () =>
            viewState.Levels["0"].StaticTexts.Count.ShouldEqual(0);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        
        private static Guid rosterScope1Id;

        private static Guid question1Id;
        private static Guid staticText1Id;
        private static Guid group1Id;
        private static Guid variable1Id;

    }
}
