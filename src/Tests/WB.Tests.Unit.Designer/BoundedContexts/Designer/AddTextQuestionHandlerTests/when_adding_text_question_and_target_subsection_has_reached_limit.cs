using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_target_subsection_has_reached_limit : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            for (int i = 0; i < 400; i++)
            {
                questionnaire.AddTextQuestion(Guid.NewGuid(),chapterId, title: $"q{i}", responsibleId:responsibleId);
            }
            
            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.AddTextQuestion(
                    questionId: existingQuestionId,
                    parentId: chapterId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    isPreFilled: isPreFilled,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    mask: null,
                    responsibleId: responsibleId));

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "section", "have", "400", "child" });
        }

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid existingQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string enablementCondition = null;
        private static string variableName = "text_question";
        private static bool isPreFilled = false;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string validationExpression = "";
        private static string validationMessage = "";
    }
}
