using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_with_max_allowed_answers_less_than_2 : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(parentGroupId, responsibleId: responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId,
                        parentGroupId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: "title",
                    variableName: "var",
                    variableLabel: null,
                    scope: QuestionScope.Interviewer,
                    enablementCondition: null,
                    hideIfDisabled: false,
                    instructions: null,
                    responsibleId: responsibleId,
                    options: new Option[]
                    {
                        new Option(Guid.NewGuid(), "1", "opt1Title"),
                        new Option(Guid.NewGuid(), "2", "opt2Title")
                    },
                    linkedToEntityId: null,
                    areAnswersOrdered: false,
                    maxAllowedAnswers: 1,
                    yesNoView: yesNoView, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));


        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__maximum_allowed_answers_should_be_more_than_one__ () =>
            new[] { "maximum allowed answers for question should be more than one" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid parentGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static bool yesNoView = false;
    }
}