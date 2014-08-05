using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneQrBarcodeQuestionHandlerTests
{
    internal class when_cloning_qr_barcode_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = sourceQuestionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.CloneQRBarcodeQuestion(questionId: questionId, title: "title", parentGroupId: chapterId,
                    variableName: "qr_barcode_question",
                variableLabel: null, isMandatory: isMandatory, enablementCondition: condition, instructions: instructions,
                    responsibleId: responsibleId, sourceQuestionId: sourceQuestionId, targetIndex: targetIndex);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QRBarcodeQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionCloned>();

        It should_raise_QRBarcodeQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_QRBarcodeQuestionAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .ParentGroupId.ShouldEqual(chapterId);

        It should_raise_QRBarcodeQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .VariableName.ShouldEqual(variableName);

        It should_raise_QRBarcodeQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .Title.ShouldEqual(title);

        It should_raise_QRBarcodeQuestionAdded_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .EnablementCondition.ShouldEqual(condition);

        It should_raise_QRBarcodeQuestionAdded_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .IsMandatory.ShouldEqual(isMandatory);

        It should_raise_QRBarcodeQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .Instructions.ShouldEqual(instructions);

        It should_raise_QRBarcodeQuestionAdded_event_with_SourceQuestionId_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .SourceQuestionId.ShouldEqual(sourceQuestionId);

        It should_raise_QRBarcodeQuestionAdded_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>()
                .TargetIndex.ShouldEqual(targetIndex);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
        private static int targetIndex = 1;
    }
}