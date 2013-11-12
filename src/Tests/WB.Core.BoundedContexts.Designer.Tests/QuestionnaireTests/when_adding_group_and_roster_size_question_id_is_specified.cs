using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_adding_group_and_roster_size_question_id_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewQuestionAdded { PublicKey = rosterSizeQuestionId, QuestionType = QuestionType.Numeric });

            eventContext = new EventContext();
        };

        Because of = () =>
            questionnaire.AddGroup(Guid.NewGuid(), responsibleId, "title", Propagate.None, rosterSizeQuestionId, null, null, null);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewGroupAdded_event = () =>
            eventContext.ShouldContainEvent<NewGroupAdded>();

        It should_raise_NewGroupAdded_event_with_IsRoster_equal_true = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .IsRoster.ShouldEqual(true);

        It should_raise_NewGroupAdded_event_with_RosterSizeQuestionId_equal_to_specified_question_id = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterSizeQuestionId;
    }
}