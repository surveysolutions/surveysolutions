using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneStaticTextHandlerTests
{
    internal class when_cloning_static_text_and_entity_with_same_id_already_exists : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new StaticTextAdded() { EntityId = sourceEntityId, ParentId = chapterId });
            questionnaire.Apply(new StaticTextAdded() {EntityId = targetEntityId, ParentId = chapterId});
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneStaticText(entityId: targetEntityId, responsibleId: responsibleId, sourceEntityId: sourceEntityId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__id_already_exists__ = () =>
             new[] { "id", "already", "exists" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid sourceEntityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid targetEntityId = Guid.Parse("33333333333333333333333333333333");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}