using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateSingleOptionQuestionHandlerTests
{
    internal class when_updating_single_option_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId,
                        chapterId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");
        };

        Because of = () =>
            questionnaire.UpdateSingleOptionQuestion(
                questionId: questionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isPreFilled: isPreFilled,
                scope: scope,
                enablementCondition: enablementCondition,
                hideIfDisabled: false,
                instructions: instructions,
                responsibleId: responsibleId,
                options: options,
                linkedToEntityId: linkedToQuestionId,
                isFilteredCombobox: isFilteredCombobox,
                cascadeFromQuestionId: сascadeFromQuestionId, 
                validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>
                {
                    new ValidationCondition {Expression = validationExpression, Message = validationMessage}
                },
                linkedFilterExpression: null, properties: Create.QuestionProperties());


        It should_contains_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ShouldNotBeNull();

        It should_contains_question_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        It should_contains_question_with_variable_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .StataExportCaption.ShouldEqual(variableName);

        It should_contains_question_with_title_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionText.ShouldEqual(title);

        It should_contains_question_with_condition_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_contains_question_with_instructions_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.ShouldEqual(instructions);

        It should_contains_question_with_featured_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Featured.ShouldEqual(isPreFilled);

        It should_contains_question_with_scope_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionScope.ShouldEqual(scope);

        It should_contains_question_with_validationExpression_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ValidationConditions.First().Expression.ShouldEqual(validationExpression);

        It should_contains_question_with_validationMessage_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ValidationConditions.First().Message.ShouldEqual(validationMessage);

        It should_contains_question_with_isFilteredCombobox_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .IsFilteredCombobox.ShouldEqual(isFilteredCombobox);

        It should_raise_NewQuestionAdded_event_with_same_options_count_as_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Answers.Count.ShouldEqual(options.Length);

        It should_raise_NewQuestionAdded_event_with_same_option_titles_as_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Answers.Select(x => x.AnswerText).ShouldContainOnly(options.Select(x => x.Title));

        It should_raise_NewQuestionAdded_event_with_same_option_values_as_specified = () =>
           questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
               .Answers.Select(x => x.AnswerValue).ShouldContainOnly(options.Select(x => x.Value));


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
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool isFilteredCombobox = false;
        private static Guid? сascadeFromQuestionId = (Guid?)null;
    }
}