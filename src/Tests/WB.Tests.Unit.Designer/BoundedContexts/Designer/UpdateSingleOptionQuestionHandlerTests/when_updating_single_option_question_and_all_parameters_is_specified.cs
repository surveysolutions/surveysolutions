using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
        }

        private void BecauseOf() =>
            questionnaire.UpdateSingleOptionQuestion(
                new UpdateSingleOptionQuestion(
                    questionnaireId: questionnaire.Id,
                    questionId: questionId,
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
                    cascadeFromQuestionId: сascadeFromQuestionId,
                    validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>()
                    {
                        new ValidationCondition {Expression = validationExpression, Message = validationMessage}
                    },
                    linkedFilterExpression: null,
                    validationExpression: null,
                    validationMessage: null,
                    showAsList: false,
                    showAsListLimit: null));


        [NUnit.Framework.Test] public void should_contains_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).Should().NotBeNull();

        [NUnit.Framework.Test] public void should_contains_question_with_QuestionId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_contains_question_with_variable_name_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .StataExportCaption.Should().Be(variableName);

        [NUnit.Framework.Test] public void should_contains_question_with_title_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionText.Should().Be(title);

        [NUnit.Framework.Test] public void should_contains_question_with_condition_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ConditionExpression.Should().Be(enablementCondition);

        [NUnit.Framework.Test] public void should_contains_question_with_instructions_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.Should().Be(instructions);

        [NUnit.Framework.Test] public void should_contains_question_with_featured_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Featured.Should().Be(isPreFilled);

        [NUnit.Framework.Test] public void should_contains_question_with_scope_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionScope.Should().Be(scope);

        [NUnit.Framework.Test] public void should_contains_question_with_validationExpression_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ValidationConditions.First().Expression.Should().Be(validationExpression);

        [NUnit.Framework.Test] public void should_contains_question_with_validationMessage_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ValidationConditions.First().Message.Should().Be(validationMessage);

        [NUnit.Framework.Test] public void should_contains_question_with_isFilteredCombobox_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .IsFilteredCombobox.Should().Be(isFilteredCombobox);

        [NUnit.Framework.Test] public void should_raise_NewQuestionAdded_event_with_same_options_count_as_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Answers.Count.Should().Be(options.Length);

        [NUnit.Framework.Test] public void should_raise_NewQuestionAdded_event_with_same_option_titles_as_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Answers.Select(x => x.AnswerText).Should().BeEquivalentTo(options.Select(x => x.Title));

        [NUnit.Framework.Test] public void should_raise_NewQuestionAdded_event_with_same_option_values_as_specified () =>
           questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
               .Answers.Select(x => x.AnswerValue).Should().BeEquivalentTo(options.Select(x => x.Value));


        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static bool isPreFilled = false;
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static string validationExpression = "some validation";
        private static string validationMessage = "validation message";
        private static Option[] options = new Option[] { new Option("1", "Option 1"), new Option("2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static Guid? сascadeFromQuestionId = (Guid?)null;
    }
}
