using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateStaticTextHandlerTests
{
    internal class when_updating_static_text_and_entity_does_not_exists : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId : entityId, parentId : chapterId));

            command = Create.Command.UpdateStaticText(
                questionnaire.PublicKey,
                entityId: notExistingEntityId,
                text: "title",
                attachmentName: "",
                responsibleId: responsibleId,
                enablementCondition: String.Empty);
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.UpdateStaticText(command));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__item_cant_found__ = () =>
             new[] { "item", "can't", "found" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));
        
        private static UpdateStaticText command;
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid notExistingEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}