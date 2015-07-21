using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_chapter_with_numeric_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() {PublicKey = chapterId, GroupText = chapterTitle});
            
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = questionId,
                GroupPublicKey = chapterId,
                QuestionText = title,
                ConditionExpression =  conditionExpression,
                Instructions = instructions,
                Mandatory =  isMandatory,
                StataExportCaption = variableName,
                CountOfDecimalPlaces = countOfDecimalPlaces,
                Featured = isPrefilled,
                IsInteger = isInteger,
                QuestionScope = questionScope,
                ValidationExpression = validationExpression,
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

        It should_raise_NumericQuestionCloned_event = () =>
            eventContext.ShouldContainEvent<NumericQuestionCloned>();

        It should_NumericQuestionCloned_event_source_question_id_be_equal_questionId = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().SourceQuestionId.ShouldEqual(questionId);

        It should_NumericQuestionCloned_event_parent_group_id_be_equal_targetGroupId = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().GroupPublicKey.ShouldEqual(targetGroupId);

        It should_NumericQuestionCloned_event_StataExportCaption_be_empty = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().StataExportCaption.ShouldEqual(string.Empty);

        It should_NumericQuestionCloned_event_QuestionText_be_equal_title = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().QuestionText.ShouldEqual(title);

        It should_NumericQuestionCloned_event_Mandatory_be_equal_isMandatory = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().Mandatory.ShouldEqual(isMandatory);

        It should_NumericQuestionCloned_event_Instructions_be_equal_instructions = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().Instructions.ShouldEqual(instructions);

        It should_NumericQuestionCloned_event_ConditionExpression_be_equal_conditionExpression = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().ConditionExpression.ShouldEqual(conditionExpression);

        It should_NumericQuestionCloned_event_Featured_be_equal_isPrefilled = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().Featured.ShouldEqual(isPrefilled);

        It should_NumericQuestionCloned_event_QuestionScope_be_equal_questionScope = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().QuestionScope.ShouldEqual(questionScope);

        It should_NumericQuestionCloned_event_CountOfDecimalPlaces_be_equal_countOfDecimalPlaces = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().CountOfDecimalPlaces.ShouldEqual(countOfDecimalPlaces);

        It should_NumericQuestionCloned_event_IsInteger_be_equal_isInteger = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().IsInteger.ShouldEqual(isInteger);

        It should_NumericQuestionCloned_event_ValidationExpression_be_equal_validationExpression = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().ValidationExpression.ShouldEqual(validationExpression);

        It should_NumericQuestionCloned_event_ValidationMessage_be_equal_validationMessage = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().ValidationMessage.ShouldEqual(validationMessage);

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
        private static int? countOfDecimalPlaces = 5;
        private static bool isPrefilled = true;
        private static bool isInteger = true;
        private static QuestionScope questionScope = QuestionScope.Interviewer;
        private static string validationExpression = "validation expression";
        private static string validationMessage = "validation message";
            
        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}