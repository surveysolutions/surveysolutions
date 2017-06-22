using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_group_inside_roster_with_linked_source_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            
            questionnaire.AddGroup(rosterId,chapterId, responsibleId: responsibleId, isRoster: true);
            questionnaire.AddGroup(groupInsideRosterId,  rosterId, responsibleId: responsibleId);
            questionnaire.AddTextQuestion(linkedSourceQuestionId,
                groupInsideRosterId,
                responsibleId);

            questionnaire.AddMultiOptionQuestion(categoricalLinkedQuestionId,
                chapterId,
                responsibleId,
                options: new Option[0],
                linkedToQuestionId: linkedSourceQuestionId);
            BecauseOf();
        }

        private void BecauseOf() => exception = Catch.Exception(() => questionnaire.DeleteGroup(rosterId, responsibleId));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__contains__ () =>
            exception.Message.ToLower().ShouldContain("contains");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__linked__ () =>
            exception.Message.ToLower().ShouldContain("linked");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__source__ () =>
            exception.Message.ToLower().ShouldContain("source");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__question__ () =>
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