using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.AddTextQuestionHandlerTests
{
    internal class when_adding_text_question_and_title_contains_substitution_to_invalid_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddTextQuestion(
                    questionId: questionId,
                    parentGroupId: chapterId,
                    title: titleWithSubstitution,
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

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title___not_existing_variable_name__as__substitution__ = () =>
            new[] { "text", "contains", "unknown", "substitution" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "notExistingVar";
        private static string titleWithSubstitution = string.Format("title with substitution - %{0}%", substitutionVariableName);
        private static string variableName = "text_question";
        private static bool isPreFilled = false;
        private static string instructions = "intructions";
        private static string enablementCondition = "";
        private static string validationExpression = "";
        private static string validationMessage = "";
    }
}