using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(
                publicKey: questionId,
                groupPublicKey: chapterId,
                questionText: "old title",
                stataExportCaption: "old_variable_name",
                instructions: "old instructions",
                conditionExpression: "old condition",
                responsibleId: responsibleId,
                questionType: QuestionType.QRBarcode
                ));
            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.UpdateQRBarcodeQuestion(questionId: questionId, title: "title",
                    variableName: "qr_barcode_question",
                variableLabel: null, enablementCondition: condition, hideIfDisabled: hideIfDisabled, instructions: instructions, 
                    responsibleId: responsibleId, 
                    scope: QuestionScope.Interviewer, 
                    validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>
                    {
                        new ValidationCondition
                        {
                            Expression = validation,
                            Message = validationMessage
                        }
                    },
                    properties: Create.QuestionProperties());

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QRBarcodeQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionUpdated>();

        It should_raise_QRBarcodeQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_QRBarcodeQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .VariableName.ShouldEqual(variableName);

        It should_raise_QRBarcodeQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .Title.ShouldEqual(title);

        It should_raise_QRBarcodeQuestionAdded_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .EnablementCondition.ShouldEqual(condition);

        It should_raise_QRBarcodeQuestionAdded_event_with_hideIfDisabled_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        It should_raise_QRBarcodeQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QRBarcodeQuestionAdded_event_with_validation_specified = () =>
           eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
               .ValidationConditions.First().Expression.ShouldEqual(validation);

        It should_raise_QRBarcodeQuestionAdded_event_with_validation_message_specified = () =>
         eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
             .ValidationConditions.First().Message.ShouldEqual(validationMessage);

        private static EventContext eventContext;
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