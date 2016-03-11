using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_enablementCondition_reference_existing_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression("roster.Max(x => x.age) > maxAge") == new[] { "roster", "age", "maxAge" });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, expressionProcessor: expressionProcessor);
            questionnaire.Apply(Create.Event.AddGroup(chapterId));
            questionnaire.Apply(Create.Event.AddGroup(rosterId, variableName: "roster", parentId: chapterId));
            questionnaire.Apply(Create.Event.GroupBecameRoster(rosterId));
            questionnaire.Apply(Create.Event.RosterChanged(rosterId, rosterType: RosterSizeSourceType.FixedTitles,
                titles: new[] { new FixedRosterTitle(1, "1"), new FixedRosterTitle(2, "2") }));
            questionnaire.Apply(Create.Event.AddTextQuestion(rosterQuestionId, parentId: rosterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(rosterQuestionId, variableName: "age"));
            questionnaire.Apply(Create.Event.AddTextQuestion(existingQuestionId, parentId: chapterId));
            questionnaire.Apply(Create.Event.UpdateNumericIntegerQuestion(existingQuestionId, variableName: "maxAge"));

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