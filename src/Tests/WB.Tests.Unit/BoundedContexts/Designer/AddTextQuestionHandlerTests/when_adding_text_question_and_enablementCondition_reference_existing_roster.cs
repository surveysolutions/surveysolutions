using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_enablementCondition_reference_existing_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(Create.Event.AddGroup(chapterId));
            questionnaire.Apply(Create.Event.AddGroup(rosterId, variableName: "roster", parentId: chapterId));
            questionnaire.Apply(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.Apply(Create.Event.RosterChanged(rosterId, rosterType: RosterSizeSourceType.FixedTitles, titles: new[] { "1", "2" }));
            questionnaire.Apply(Create.Event.AddTextQuestion(rosterQuestionId, parentId: rosterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(rosterQuestionId, variableName: "age"));
            questionnaire.Apply(Create.Event.AddTextQuestion(existingQuestionId, parentId: chapterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(existingQuestionId, variableName: "maxAge"));

            RegisterExpressionProcessorMock("roster.Max(x => x.age) > maxAge", new[] { "roster", "age", "maxAge" });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            questionnaire.AddTextQuestion(
                questionId: questionId,
                parentGroupId: chapterId,
                title: "title",
                variableName: "text_question",
                variableLabel: null,
                isMandatory: false,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: "roster.Max(x => x.age) > maxAge",
                validationExpression: "",
                validationMessage: "",
                instructions: "intructions",
                mask: null,
                responsibleId: responsibleId,
                index: nullIndex);

        It should_rise_NewQuestionAdded_event_with_questionId = () =>
            eventContext.ShouldContainEvent<NewQuestionAdded>(x => x.PublicKey == questionId);

        It should_not_rise_QuestionnaireItemMoved_event_with_questionId = () =>
           eventContext.ShouldNotContainEvent<QuestionnaireItemMoved>(x => x.PublicKey == questionId);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid existingQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid notExistingQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static int? nullIndex = null;
    }
}