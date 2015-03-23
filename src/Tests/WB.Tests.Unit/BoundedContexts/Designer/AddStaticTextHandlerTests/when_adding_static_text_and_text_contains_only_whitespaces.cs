using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.AddStaticTextHandlerTests
{
    internal class when_adding_static_text_and_text_contains_only_whitespaces : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddStaticTextAndMoveIfNeeded(entityId: entityId, parentId: chapterId, text: "  ", responsibleId: responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__empty__text__ = () =>
             new[] { "empty", "text" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}