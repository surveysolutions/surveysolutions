using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_roster_group_by_question_and_roster_size_question_is_under_another_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.Apply(new NumericQuestionAdded { PublicKey = rosterSizeQuestionId, IsInteger = true, GroupPublicKey = anotherRosterId });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, anotherRosterId, true,
                    RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null);

        It should_raise_NewGroupAdded_event = () =>
           eventContext.ShouldContainEvent<NewGroupAdded>();

        It should_raise_NewGroupAdded_event_with_GroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .PublicKey.ShouldEqual(groupId);

        It should_raise_NewGroupAdded_event_with_ParentGroupId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ParentGroupPublicKey.ShouldEqual(anotherRosterId);

        It should_raise_NewGroupAdded_event_with_Title_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .GroupText.ShouldEqual("title");

        It should_raise_NewGroupAdded_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<NewGroupAdded>()
                .ResponsibleId.ShouldEqual(responsibleId);


        private static EventContext eventContext;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
        private static Questionnaire questionnaire;
        private static Guid anotherRosterId;
    }
}