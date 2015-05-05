using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_itself_in_enablement_condition : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(Create.Event.AddGroup(groupId: chapterId));
            questionnaire.Apply(Create.Event.AddTextQuestion(questionId : questionToBeDeleted, parentId : chapterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(questionToBeDeleted, "q", enablementCondition: "q > 10"));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };


        Because of = () => 
            questionnaire.DeleteQuestion(questionToBeDeleted, responsibleId);

        It should_raise_QuestionDeleted_event = () =>
            eventContext.ShouldContainEvent<QuestionDeleted>();

        It should_raise_QuestionDeleted_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionDeleted>().QuestionId.ShouldEqual(questionToBeDeleted);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static readonly Guid questionToBeDeleted = Guid.Parse("21111111111111111111111111111111");
        private static readonly Guid responsibleId= Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}