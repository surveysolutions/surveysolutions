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
    internal class when_handling_MacroUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            evnt = Create.Event.MacroUpdated(questionnaireId, entityId, name, content, description);
            
            questionnaire = Create.QuestionnaireDocument(questionnaireId);
            
            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage => storage.GetById(Moq.It.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
            denormalizer.Handle(Create.Event.MacroAdded(questionnaireId, entityId));
        };

        Because of = () => denormalizer.Handle(evnt);

        It should_not_add_extra_macro = () =>
            questionnaire.Macros.Count.ShouldEqual(1);

        It should_update_macro_with_specified_name = () =>
            questionnaire.Macros[entityId].Name.ShouldEqual(name);

        It should_update_macro_with_specifiedy_content = () =>
            questionnaire.Macros[entityId].Content.ShouldEqual(content);

        It should_update_macro_with_specified_description = () =>
            questionnaire.Macros[entityId].Description.ShouldEqual(description);

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string name = "macros";
        private static readonly string content = "macros content";
        private static readonly string description = "macros description";
        private static QuestionnaireDenormalizer denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static IPublishedEvent<MacroUpdated> evnt;
    }
}