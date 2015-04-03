using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_chapter_with_qr_barcode_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() {PublicKey = chapterId, GroupText = chapterTitle});
            
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
                ParentGroupId = chapterId,
                Title = title,
                EnablementCondition =  conditionExpression,
                Instructions = instructions,
                IsMandatory =  isMandatory,
                VariableName = variableName,
                ValidationExpression = validation,
                ValidationMessage = validationMessage
            });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneGroup(groupId: targetGroupId, responsibleId: responsibleId, sourceGroupId: chapterId, targetIndex: targetIndex);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_GroupCloned_event = () =>
             eventContext.ShouldContainEvent<GroupCloned>();

        It should_GroupCloned_event_public_key_be_equal_targetGroupId = () =>
            GetSingleEvent<GroupCloned>(eventContext).PublicKey.ShouldEqual(targetGroupId);

        It should_GroupCloned_event_group_title_be_equal_chapterTitle = () =>
            GetSingleEvent<GroupCloned>(eventContext).GroupText.ShouldEqual(chapterTitle);

        It should_GroupCloned_event_source_group_id_be_equal_chapterId = () =>
            GetSingleEvent<GroupCloned>(eventContext).SourceGroupId.ShouldEqual(chapterId);

        It should_GroupCloned_event_parent_group_id_be_equal_to_null = () =>
            GetSingleEvent<GroupCloned>(eventContext).ParentGroupPublicKey.ShouldEqual(questionnaireId);

        It should_GroupCloned_event_target_index_be_equal_targetIndex = () =>
            GetSingleEvent<GroupCloned>(eventContext).TargetIndex.ShouldEqual(targetIndex);

        It should_raise_QRBarcodeQuestionCloned_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionCloned>();

        It should_QRBarcodeQuestionCloned_event_source_question_id_be_equal_questionId = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().SourceQuestionId.ShouldEqual(questionId);

        It should_QRBarcodeQuestionCloned_event_parent_group_id_be_equal_targetGroupId = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().ParentGroupId.ShouldEqual(targetGroupId);

        It should_QRBarcodeQuestionCloned_event_StataExportCaption_be_empty = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().VariableName.ShouldEqual(string.Empty);

        It should_QRBarcodeQuestionCloned_event_QuestionText_be_equal_title = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().Title.ShouldEqual(title);

        It should_QRBarcodeQuestionCloned_event_Mandatory_be_equal_isMandatory = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().IsMandatory.ShouldEqual(isMandatory);

        It should_QRBarcodeQuestionCloned_event_Instructions_be_equal_instructions = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().Instructions.ShouldEqual(instructions);

        It should_QRBarcodeQuestionCloned_event_ConditionExpression_be_equal_conditionExpression = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().EnablementCondition.ShouldEqual(conditionExpression);

        It should_QRBarcodeQuestionCloned_event_ValidationExpression_be_equal_validation = () =>
           eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().ValidationExpression.ShouldEqual(validation);

        It should_QRBarcodeQuestionCloned_event_ValidationMessage_be_equal_validationMessage = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionCloned>().ValidationMessage.ShouldEqual(validationMessage);

        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string chapterTitle = "chapter title";
        private static Guid questionId = Guid.Parse("22222222222222222222222222222222");
        private static string title = "text question title";
        private static string variableName = "var_name";
        private static string conditionExpression = "condition exptession";
        private static string instructions = "instructions";
        private static bool isMandatory = true;
        private static string validation = "validation";
        private static string validationMessage = "validationMessage";

        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}