using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_moving_roster_in_paralell_roster_scope_but_roster_size_question_is_left: QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = Guid.Parse("31111111111111111111111111111111");
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);

            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));

            questionnaire.Apply(new NewGroupAdded { PublicKey = parallelRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parallelRosterId));

            questionnaire.Apply(new NumericQuestionAdded { PublicKey = rosterSizeQuestionId, IsInteger = true, GroupPublicKey = parallelRosterId });


            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = parallelRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, groupId));
            questionnaire.Apply(new RosterChanged(responsibleId, groupId, rosterSizeQuestionId, RosterSizeSourceType.Question, null, null));
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.MoveGroup(groupId, anotherRosterId, 0, responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__with__ = () =>
            exception.Message.ToLower().ShouldContain("size");

        It should_throw_exception_with_message_containting__into__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__roster_twice__ = () =>
            exception.Message.ToLower().Split().Count(s => s == "roster").ShouldEqual(2);

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid parallelRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}
