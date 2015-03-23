using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_group_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parentRosterId));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: title, variableName: null, rosterSizeQuestionId: null, description: description,
                    condition: condition, parentGroupId: parentRosterId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null, index: index);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewGroupAdded_event = () =>
            eventContext.ShouldContainEvent<NewGroupAdded>();

        It should_raise_NewGroupAdded_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .PublicKey.ShouldEqual(groupId);

        It should_raise_NewGroupAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ParentGroupPublicKey.ShouldEqual(parentRosterId);

        It should_raise_NewGroupAdded_event_with_ConditionExpression_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ConditionExpression.ShouldEqual(condition);

        It should_raise_NewGroupAdded_event_with_Title_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .GroupText.ShouldEqual(title);

        It should_raise_NewGroupAdded_event_with_Description_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .Description.ShouldEqual(description);

        It should_raise_NewGroupAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ResponsibleId.ShouldEqual(responsibleId);

        It should_raise_QuestionnaireItemMoved_event = () =>
            eventContext.ShouldContainEvent<QuestionnaireItemMoved>();

        It should_raise_QuestionnaireItemMoved_event_with_GroupKey_specified = () =>
           eventContext.GetSingleEvent<QuestionnaireItemMoved>().GroupKey.ShouldEqual(parentRosterId);

        It should_raise_QuestionnaireItemMoved_event_with_PublicKey_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().PublicKey.ShouldEqual(groupId);

        It should_raise_QuestionnaireItemMoved_event_with_TargetIndex_specified = () =>
            eventContext.GetSingleEvent<QuestionnaireItemMoved>().TargetIndex.ShouldEqual(index);

        It should_raise_QuestionnaireItemMoved_event_with_ResponsibleId_specified = () =>
           eventContext.GetSingleEvent<QuestionnaireItemMoved>().ResponsibleId.ShouldEqual(responsibleId);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static string condition = "some condition";
        private static string title = "title";
        private static string description = "description";
        private static EventContext eventContext;
        private static int index = 5;
    }
}