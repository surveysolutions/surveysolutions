using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Aggregates;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_with_self_referencing_valiation_expression : QuestionnaireTestsContext
    {
        private [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            BecauseOf();
        }


        private void BecauseOf() =>
            questionnaire.AddTextQuestion(
                questionId: questionId,
                parentId: chapterId,
                title: "title",
                variableName: variableName,
                variableLabel: null,
                isPreFilled: false,
                scope: QuestionScope.Interviewer,
                enablementCondition: "",
                validationExpression: validationExpression,
                validationMessage: "aaaa",
                instructions: "intructions",
                mask: null,
                responsibleId: responsibleId);

        [NUnit.Framework.Test] public void should_contains_TextQuestion_with_QuestionId_specified () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        [NUnit.Framework.Test] public void should_contains_TextQuestion_with_validationExpression_specified () =>
            questionnaire.QuestionnaireDocument.Find<TextQuestion>(questionId)
                .ValidationConditions.First().Expression.ShouldEqual(validationExpression);

        private static Questionnaire questionnaire;

        private static string variableName = "var";
        private static string validationExpression = string.Format("{0} == \"Hello\"", variableName);

        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}