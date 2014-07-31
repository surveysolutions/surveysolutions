using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_chapter_with_static_text : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId, questionnaireId: questionnaireId);
            questionnaire.Apply(new NewGroupAdded {PublicKey = chapterId, GroupText = chapterTitle});
            
            questionnaire.Apply(new StaticTextAdded { EntityId = staticTextId, ParentId = chapterId, Text = title });

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

        It should_raise_StaticTextCloned_event = () =>
            eventContext.ShouldContainEvent<StaticTextCloned>();

        It should_StaticTextCloned_event_source_question_id_be_equal_questionId = () =>
            eventContext.GetSingleEvent<StaticTextCloned>().SourceEntityId.ShouldEqual(staticTextId);

        It should_StaticTextCloned_event_parent_group_id_be_equal_targetGroupId = () =>
            eventContext.GetSingleEvent<StaticTextCloned>().ParentId.ShouldEqual(targetGroupId);

        It should_StaticTextCloned_event_QuestionText_be_equal_title = () =>
            eventContext.GetSingleEvent<StaticTextCloned>().Text.ShouldEqual(title);

        private static Questionnaire questionnaire;
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string chapterTitle = "chapter title";
        private static Guid staticTextId = Guid.Parse("22222222222222222222222222222222");
        private static string title = "some static text";
            
        private static int targetIndex = 0;
        private static EventContext eventContext;
    }
}