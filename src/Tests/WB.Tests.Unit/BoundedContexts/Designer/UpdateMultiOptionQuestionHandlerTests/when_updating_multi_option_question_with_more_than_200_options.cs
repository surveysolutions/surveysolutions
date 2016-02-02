using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_with_more_than_200_options : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            int incrementer = 0;
            options = new List<Option>(
                    new Option[201].Select(
                        option => new Option(new Guid(), incrementer.ToString(), (incrementer++).ToString()))).ToArray();

            questionnaire = CreateQuestionnaireWithOneQuestion(responsibleId: responsibleId, questionId: questionId);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: title,
                    variableName: variableName,
                    variableLabel: null,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    validationExpression: validationExpression,
                    validationMessage: validationMessage,
                    instructions: instructions,
                    responsibleId: responsibleId, 
                    options: options,
                    linkedToEntityId: linkedToQuestionId,
                    areAnswersOrdered: false,
                    maxAllowedAnswers: 5,
                    yesNoView: false));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_about_riching_options_limit = () =>
            new[] { "more than", 200.ToString(), "options" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid? linkedToQuestionId = (Guid?)null;
        private static string variableName = "multi_var";
        private static string title = "title";
        private static string instructions = "instructions";
        private static string enablementCondition = "";
        private static string validationExpression = "";
        private static string validationMessage = "";
        private static Option[] options;
        
        private static QuestionScope scope = QuestionScope.Interviewer;
    }
}