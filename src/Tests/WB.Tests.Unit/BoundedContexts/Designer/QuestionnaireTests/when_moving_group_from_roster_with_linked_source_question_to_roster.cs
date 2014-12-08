﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_moving_group_from_roster_with_linked_source_question_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = categoricalLinkedQuestionId,
                QuestionType = QuestionType.MultyOption,
                GroupPublicKey = chapterId,
                LinkedToQuestionId = linkedSourceQuestionId
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster1Id, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster1Id));
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster2Id, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster2Id));
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupInsideRosterId, ParentGroupPublicKey = roster1Id });
            questionnaire.Apply(new NewQuestionAdded()
            {
                PublicKey = linkedSourceQuestionId,
                GroupPublicKey = groupInsideRosterId,
                QuestionType = QuestionType.Text
            });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.MoveGroup(groupId: groupInsideRosterId, targetGroupId: roster2Id, responsibleId: responsibleId, targetIndex:0);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .PublicKey.ShouldEqual(groupInsideRosterId);

        It should_raise_QuestionnaireItemMoved_event_with_TargetGroupId_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>()
                .GroupKey.ShouldEqual(roster2Id);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("012EEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupInsideRosterId = Guid.Parse("ABCEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid categoricalLinkedQuestionId = Guid.Parse("FFFCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid linkedSourceQuestionId = Guid.Parse("AAACCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static EventContext eventContext;
    }
}