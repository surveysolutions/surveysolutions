using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_and_it_does_not_exists_in_questionnaire : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId,
                        chapterId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");
            BecauseOf();

            exception.Message.ToLower().ToSeparateWords().Should().Contain(new[] { "question", "can't", "found" });
        }

        private void BecauseOf() =>
            exception = Assert.Throws<QuestionnaireException>(() =>
                questionnaire.UpdateSingleOptionQuestion(
                    new UpdateSingleOptionQuestion(
                        questionnaireId: questionnaire.Id,
                        questionId: notExistingQuestionId,
                        commonQuestionParameters: new CommonQuestionParameters()
                        {
                            Title = title,
                            VariableName = variableName,
                            VariableLabel = null,
                            EnablementCondition = enablementCondition,
                            Instructions = instructions,
                            HideIfDisabled = false
                        },

                        isPreFilled: isPreFilled,
                        scope: scope,
                        responsibleId: responsibleId,
                        options: options,
                        linkedToEntityId: linkedToQuestionId,
                        isFilteredCombobox: isFilteredCombobox,
                        cascadeFromQuestionId: cascadeFromQuestionId,
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                        linkedFilterExpression: null,
                        validationExpression: null,
                        validationMessage: null,
                        showAsList: false,
                        showAsListThreshold: null)));

        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid notExistingQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static Option[] options = new Option[] { new Option("1", "Option 1"), new Option("2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static Guid? cascadeFromQuestionId = (Guid?)null;
    }
}
