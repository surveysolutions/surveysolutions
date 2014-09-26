using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_chapter_with_multimedia_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() { PublicKey = chapterId, GroupText = chapterTitle });

            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = questionId,
                GroupPublicKey = chapterId,
                QuestionText = title,
                ConditionExpression = conditionExpression,
                Instructions = instructions,
                Mandatory = isMandatory,
                StataExportCaption = variableName
            });

            questionnaire.Apply(new MultimediaQuestionUpdated()
            {
                QuestionId = questionId,
                Title = title,
                EnablementCondition = conditionExpression,
                Instructions = instructions,
                IsMandatory = isMandatory,
                VariableName = variableName
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

        It should_QuestionCloned_event_public_key_be_not_equal_questionId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().PublicKey.ShouldNotEqual(questionId);

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

        It should_raise_MultimediaQuestionUpdated_event = () =>
            eventContext.ShouldContainEvent<MultimediaQuestionUpdated>();

        It should_MultimediaQuestionUpdated_event_question_id_be_not_equal_questionId = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().QuestionId.ShouldNotEqual(questionId);

        It should_MultimediaQuestionUpdated_event_VariableName_be_empty = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().VariableName.ShouldEqual(string.Empty);

        It should_MultimediaQuestionUpdated_event_Title_be_equal_title = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().Title.ShouldEqual(title);

        It should_MultimediaQuestionUpdated_event_IsMandatory_be_equal_isMandatory = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().IsMandatory.ShouldEqual(isMandatory);

        It should_MultimediaQuestionUpdated_event_Instructions_be_equal_instructions = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().Instructions.ShouldEqual(instructions);

        It should_MultimediaQuestionUpdated_event_EnablementCondition_be_equal_conditionExpression = () =>
            eventContext.GetSingleEvent<MultimediaQuestionUpdated>().EnablementCondition.ShouldEqual(conditionExpression);

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

        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}
