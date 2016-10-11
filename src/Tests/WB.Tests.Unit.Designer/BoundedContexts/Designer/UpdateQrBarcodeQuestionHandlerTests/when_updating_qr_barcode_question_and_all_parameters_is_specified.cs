using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions",
                enablementCondition: "old condition",
                responsibleId: responsibleId);
        };

        Because of = () =>            
                questionnaire.UpdateQRBarcodeQuestion(
                    new UpdateQRBarcodeQuestion(
                        questionnaire.Id,
                        questionId: questionId, 
                        commonQuestionParameters:new CommonQuestionParameters()
                        {
                            Title = "title",
                            VariableName = "qr_barcode_question",
                            EnablementCondition = condition,
                            Instructions = instructions,
                            HideIfDisabled = hideIfDisabled
                        }, 
                        validationExpression:null,
                        validationMessage:null,
                        responsibleId: responsibleId, 
                        scope: QuestionScope.Interviewer, 
                        validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>
                        {
                            new ValidationCondition
                           {
                                Expression = validation,
                                Message = validationMessage
                            }
                        }));

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
                .ConditionExpression.ShouldEqual(condition);

        It should_contains_question_with_hideIfDisabled_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        It should_contains_question_with_instructions_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.ShouldEqual(instructions);

        It should_contains_question_with_validation_specified = () =>
           questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
               .ValidationConditions.First().Expression.ShouldEqual(validation);

        It should_contains_question_with_validation_message_specified = () =>
           questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
               .ValidationConditions.First().Message.ShouldEqual(validationMessage);

        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
        private static bool hideIfDisabled = true;
        private static string validation = "validation";
        private static string validationMessage = "validationMessage";
    }
}