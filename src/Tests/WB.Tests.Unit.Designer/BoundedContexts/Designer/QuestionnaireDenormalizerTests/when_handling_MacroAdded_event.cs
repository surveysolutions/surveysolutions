using System;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_MacroAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.MacroAdded(questionnaireId, entityId);

            questionnaire = Create.QuestionnaireDocument(questionnaireId);

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage => storage.GetById(Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_add_one_macro = () =>
            questionnaire.Macros.Count.ShouldEqual(1);

        It should_add_macro_with_key_equals_entity_id = () =>
           questionnaire.Macros.ContainsKey(entityId).ShouldBeTrue();

        It should_add_macro_with_empty_name = () =>
           questionnaire.Macros[entityId].Name.ShouldBeNull();

        It should_add_macro_with_empty_content = () =>
           questionnaire.Macros[entityId].Content.ShouldBeNull();

        It should_add_macro_with_empty_description = () =>
           questionnaire.Macros[entityId].Description.ShouldBeNull();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static QuestionnaireDenormalizer denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<MacroAdded> evnt;
    }
}