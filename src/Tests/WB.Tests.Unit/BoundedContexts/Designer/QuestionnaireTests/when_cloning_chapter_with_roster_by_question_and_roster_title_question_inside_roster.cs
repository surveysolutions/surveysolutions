using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_chapter_with_roster_by_question_and_roster_title_question_inside_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() {PublicKey = chapterId, GroupText = chapterTitle});
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = chapterId,
                QuestionText = rosterSizeQuestionTitle,
                IsInteger = isRosterSizeQuestionInteger
            });
            questionnaire.Apply(new NewGroupAdded()
            {
                PublicKey = rosterId,
                GroupText = rosterTitle,
                ParentGroupPublicKey = chapterId
            });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: rosterId)
                {
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterTitleQuestionId
                });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = rosterId,
                QuestionType = QuestionType.Text,
                QuestionText = rosterTitleQuestionTitle
            });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneGroup(groupId: targetGroupId, responsibleId: responsibleId, sourceGroupId: chapterId, targetIndex: targetIndex);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_2_GroupCloned_event = () =>
            GetSpecificEvents<GroupCloned>(eventContext).Count().ShouldEqual(2);

        It should_first_GroupCloned_event_public_key_be_equal_targetGroupId = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(0).PublicKey.ShouldEqual(targetGroupId);

        It should_first_GroupCloned_event_group_title_be_equal_chapterTitle = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(0).GroupText.ShouldEqual(chapterTitle);

        It should_first_GroupCloned_event_source_group_id_be_equal_chapterId = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(0).SourceGroupId.ShouldEqual(chapterId);

        It should_first_GroupCloned_event_parent_group_id_be_equal_to_null = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(0).ParentGroupPublicKey.ShouldEqual(questionnaireId);

        It should_first_GroupCloned_event_target_index_be_equal_targetIndex = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(0).TargetIndex.ShouldEqual(targetIndex);

        It should_second_GroupCloned_event_source_group_id_be_equal_rosterId = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(1).SourceGroupId.ShouldEqual(rosterId);

        It should_second_GroupCloned_event_group_title_be_equal_rosterTitle = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(1).GroupText.ShouldEqual(rosterTitle);

        It should_second_GroupCloned_event__parent_group_id_be_equal_targetGroupId = () =>
            GetSpecificEvents<GroupCloned>(eventContext).ElementAt(1).ParentGroupPublicKey.ShouldEqual(targetGroupId);

        It should_raise_GroupBecameARoster_event = () =>
            eventContext.ShouldContainEvent<GroupBecameARoster>();

        It should_raise_RosterChanged_event = () =>
            eventContext.ShouldContainEvent<RosterChanged>();

        It should_RosterChanged_event_roster_size_source_be_equal_to_question = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterSizeSource.ShouldEqual(RosterSizeSourceType.Question);

        It should_RosterChanged_event_roster_size_qiestion_id_be_equal_to_rosterSizeQuestionId = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterSizeQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_RosterChanged_event_roster_title_qiestion_id_be_null = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterTitleQuestionId.ShouldBeNull();

        It should_raise_NumericQuestionCloned_event = () =>
            eventContext.ShouldContainEvent<NumericQuestionCloned>();

        It should_NumericQuestionCloned_SourceQuestionId_be_equal_to_rosterSizeQuestionId = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().SourceQuestionId.ShouldEqual(rosterSizeQuestionId);

        It should_NumericQuestionCloned_event_question_text_be_equal_to_rosterSizeQuestionTitle = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().QuestionText.ShouldEqual(rosterSizeQuestionTitle);

        It should_NumericQuestionCloned_event_is_integer_be_equal_to_isRosterSizeQuestionInteger = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().IsInteger.ShouldEqual(isRosterSizeQuestionInteger);

        It should_NumericQuestionCloned_event_group_id_be_equal_to_targetGroupId = () =>
            eventContext.GetSingleEvent<NumericQuestionCloned>().GroupPublicKey.ShouldEqual(targetGroupId);

        It should_raise_QuestionCloned_event = () =>
           eventContext.ShouldContainEvent<QuestionCloned>();

        It should_QuestionCloned_SourceQuestionId_be_equal_to_rosterTitleQuestionId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().SourceQuestionId.ShouldEqual(rosterTitleQuestionId);

        It should_QuestionCloned_event_question_type_be_equal_to_text = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionType.ShouldEqual(QuestionType.Text);

        It should_QuestionCloned_event_question_text_be_equal_textQuestionTitle = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionText.ShouldEqual(rosterTitleQuestionTitle);

        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static string chapterTitle = "chapter title";
        private static string rosterTitle = "roster title";
        private static string rosterTitleQuestionTitle = "roster title question title";
        private static string rosterSizeQuestionTitle = "roster size question title";
        private static bool isRosterSizeQuestionInteger = true;
        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}