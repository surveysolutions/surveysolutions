using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_group_with_roster_to_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId});
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster1Id, ParentGroupPublicKey = groupId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster1Id));
            questionnaire.Apply(new NewGroupAdded { PublicKey = roster2Id, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, roster2Id));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(groupId, roster2Id, 0, responsibleId));
        
        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__with__ = () =>
            exception.Message.ToLower().ShouldContain("with");

        It should_throw_exception_with_message_containting__into__ = () =>
            exception.Message.ToLower().ShouldContain("into");

        It should_throw_exception_with_message_containting__roster_twice__ = () =>
            exception.Message.ToLower().Split().Count(s => s == "roster").ShouldEqual(2);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid roster1Id = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid roster2Id = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}