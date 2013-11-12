using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_adding_group_and_roster_size_question_id_is_not_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterSizeQuestionId = null;

            questionnaire = CreateQuestionnaire(responsibleId);

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

        It should_raise_NewGroupAdded_event_with_IsRoster_equal_false = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .IsRoster.ShouldEqual(false); 

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid? rosterSizeQuestionId;
    }
}