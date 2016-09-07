using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_under_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(publicKey: rosterSizeQuestionId, groupPublicKey: chapterId, isInteger: true));
            
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = parentRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, parentRosterId));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddGroupAndMoveIfNeeded(groupId: groupId, responsibleId: responsibleId, title: title, variableName: null, 
                    rosterSizeQuestionId: rosterSizeQuestionId, description: description, condition: null, hideIfDisabled: false, parentGroupId: parentRosterId, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        It should_raise_NewGroupAdded_event = () =>
            eventContext.ShouldContainEvent<NewGroupAdded>();

        It should_raise_NewGroupAdded_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .PublicKey.ShouldEqual(groupId);

        It should_raise_NewGroupAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ParentGroupPublicKey.ShouldEqual(parentRosterId);

        It should_raise_NewGroupAdded_event_with_Title_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .GroupText.ShouldEqual(title);

        It should_raise_NewGroupAdded_event_with_Description_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .Description.ShouldEqual(description);

        It should_raise_NewGroupAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ResponsibleId.ShouldEqual(responsibleId);

        private static Questionnaire questionnaire;
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid parentRosterId=Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterSizeQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static EventContext eventContext;
        private static string title = "title";
        private static string description = "description";
    }
}