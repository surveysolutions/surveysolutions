using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_chapter_with_single_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() {PublicKey = chapterId, GroupText = chapterTitle});
            
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = questionId,
                GroupPublicKey = chapterId,
                QuestionText = title,
                ConditionExpression =  conditionExpression,
                Instructions = instructions,
                Mandatory =  isMandatory,
                StataExportCaption = variableName,
                Featured = isPrefilled,
                QuestionScope = questionScope,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                LinkedToQuestionId = linkedToQuestionId,
                Answers = answers,
                QuestionType = questionType,
                IsFilteredCombobox = isFilteredCombobox
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

        It should_raise_QuestionCloned_event = () =>
            eventContext.ShouldContainEvent<QuestionCloned>();

        It should_QuestionCloned_event_source_question_id_be_equal_questionId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().SourceQuestionId.ShouldEqual(questionId);

        It should_QuestionCloned_event_parent_group_id_be_equal_targetGroupId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().GroupPublicKey.ShouldEqual(targetGroupId);

        It should_QuestionCloned_event_StataExportCaption_be_empty = () =>
            eventContext.GetSingleEvent<QuestionCloned>().StataExportCaption.ShouldEqual(string.Empty);

        It should_QuestionCloned_event_QuestionText_be_equal_title = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionText.ShouldEqual(title);

        It should_QuestionCloned_event_Mandatory_be_equal_isMandatory = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Mandatory.ShouldEqual(isMandatory);

        It should_QuestionCloned_event_Instructions_be_equal_instructions = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Instructions.ShouldEqual(instructions);

        It should_QuestionCloned_event_ConditionExpression_be_equal_conditionExpression = () =>
            eventContext.GetSingleEvent<QuestionCloned>().ConditionExpression.ShouldEqual(conditionExpression);

        It should_QuestionCloned_event_Featured_be_equal_isPrefilled = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Featured.ShouldEqual(isPrefilled);

        It should_QuestionCloned_event_Featured_be_equal_isFilteredCombobox = () =>
            eventContext.GetSingleEvent<QuestionCloned>().IsFilteredCombobox.ShouldEqual(isFilteredCombobox);

        It should_QuestionCloned_event_QuestionType_be_equal_questionType = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionType.ShouldEqual(questionType);

        It should_QuestionCloned_event_QuestionScope_be_equal_questionScope = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionScope.ShouldEqual(questionScope);

        It should_QuestionCloned_event_ValidationExpression_be_equal_validationExpression = () =>
            eventContext.GetSingleEvent<QuestionCloned>().ValidationExpression.ShouldEqual(validationExpression);

        It should_QuestionCloned_event_ValidationMessage_be_equal_validationMessage = () =>
            eventContext.GetSingleEvent<QuestionCloned>().ValidationMessage.ShouldEqual(validationMessage);

        It should_QuestionCloned_event_LinkedToQuestionId_be_equal_linkedToQuestionId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().LinkedToQuestionId.ShouldEqual(linkedToQuestionId);

        It should_QuestionCloned_event_Answers_not_be_empty = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Answers.ShouldNotBeEmpty();

        It should_QuestionCloned_event_Answers_have_2_answers = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Answers.Count().ShouldEqual(answers.Count());

        It should_QuestionCloned_event_first_Answer_be_equals_to_first_answer = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Answers[0].ShouldBeLike(answers[0]);

        It should_QuestionCloned_event_second_Answer_be_equals_to_second_answer = () =>
            eventContext.GetSingleEvent<QuestionCloned>().Answers[1].ShouldBeLike(answers[1]);

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
        private static bool isPrefilled = true;
        private static QuestionScope questionScope = QuestionScope.Interviewer;
        private static string validationExpression = "validation expression";
        private static string validationMessage = "validation message";
        private static QuestionType questionType = QuestionType.SingleOption;
        private static Guid? linkedToQuestionId = null;
        private static bool isFilteredCombobox = true;

        private static Answer[] answers = new[]
        {
            new Answer() {AnswerValue = "answer1", AnswerText = "answer 1"},
            new Answer() {AnswerValue = "answer2", AnswerText = "answer 2"}
        };
            
        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}