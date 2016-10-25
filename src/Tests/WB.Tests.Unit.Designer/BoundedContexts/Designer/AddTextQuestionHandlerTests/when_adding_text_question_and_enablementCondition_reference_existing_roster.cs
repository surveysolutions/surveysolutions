using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_enablementCondition_reference_existing_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var expressionProcessor = Mock.Of<IExpressionProcessor>(processor
                => processor.GetIdentifiersUsedInExpression("roster1.Max(x => x.age) > maxAge") == new[] { "roster", "age", "maxAge" });

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, expressionProcessor: expressionProcessor);

            questionnaire.AddGroup(chapterId, responsibleId: responsibleId);
            questionnaire.AddGroup(rosterId, variableName: "roster1", parentGroupId: chapterId, responsibleId: responsibleId,
                isRoster:true, rosterSourceType: RosterSizeSourceType.FixedTitles, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2") });

            questionnaire.AddNumericQuestion(rosterQuestionId, rosterId, responsibleId, variableName: "age");
            questionnaire.AddNumericQuestion(existingQuestionId, chapterId, responsibleId, variableName: "maxAge");
        };

        Because of = () =>
            questionnaire.AddTextQuestion(
                questionId: questionId,
                parentId: chapterId,
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

        It should_contains_TextQuestion_with_questionId = () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId).ShouldNotBeNull();

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