using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_cloning_chapter_with_roster_by_fixed_titles_and_text_question_inside_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded() {PublicKey = chapterId, GroupText = chapterTitle});
            questionnaire.Apply(new NewGroupAdded()
            {
                PublicKey = rosterId,
                GroupText = rosterTitle,
                ParentGroupPublicKey = chapterId
            });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: rosterId)
            {
                RosterSizeQuestionId = null,
                RosterSizeSource = RosterSizeSourceType.FixedTitles,
                FixedRosterTitles = rosterFixedTitles,
                RosterTitleQuestionId = null
            });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = textQuestionId,
                GroupPublicKey = rosterId,
                QuestionType = QuestionType.Text,
                QuestionText = textQuestionTitle
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

        It should_RosterChanged_event_roster_size_source_be_equal_to_fixed_titles = () =>
            eventContext.GetSingleEvent<RosterChanged>().RosterSizeSource.ShouldEqual(RosterSizeSourceType.FixedTitles);

        It should_RosterChanged_event_roster_fixed_title_be_equal_to_rosterFixedTitles = () =>
            eventContext.GetSingleEvent<RosterChanged>().FixedRosterTitles.ShouldEqual(rosterFixedTitles);

        It should_raise_QuestionCloned_event = () =>
            eventContext.ShouldContainEvent<QuestionCloned>();

        It should_QuestionCloned_event_SourceQuestionId_be_equal_to_textQuestionId = () =>
            eventContext.GetSingleEvent<QuestionCloned>().SourceQuestionId.ShouldEqual(textQuestionId);

        It should_QuestionCloned_event_question_type_be_equal_to_text = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionType.ShouldEqual(QuestionType.Text);

        It should_QuestionCloned_event_question_text_be_equal_textQuestionTitle = () =>
            eventContext.GetSingleEvent<QuestionCloned>().QuestionText.ShouldEqual(textQuestionTitle);

        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string chapterTitle = "chapter title";
        private static string rosterTitle = "roster title";
        private static string textQuestionTitle = "text question title";
        private static int targetIndex = 0;
        private static Dictionary<decimal, string> rosterFixedTitles = new Dictionary<decimal, string>{ { 1, "title 1" }, { 2, "title 2" } };
        private static EventContext eventContext;
    }
}