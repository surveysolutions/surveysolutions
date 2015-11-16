using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_group_inside_roster_with_linked_source_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                publicKey: categoricalLinkedQuestionId,
                groupPublicKey: chapterId,
                questionType: QuestionType.MultyOption,
                linkedToQuestionId:linkedSourceQuestionId
            ));
            
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupInsideRosterId, ParentGroupPublicKey = rosterId });
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                publicKey : linkedSourceQuestionId,
                groupPublicKey : groupInsideRosterId,
                questionType : QuestionType.Text
            ));
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.DeleteGroup(rosterId, responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__contains__ = () =>
            exception.Message.ToLower().ShouldContain("contains");

        It should_throw_exception_with_message_containting__linked__ = () =>
            exception.Message.ToLower().ShouldContain("linked");

        It should_throw_exception_with_message_containting__source__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupInsideRosterId = Guid.Parse("ABCEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid categoricalLinkedQuestionId = Guid.Parse("FFFCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid linkedSourceQuestionId = Guid.Parse("AAACCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}