using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextDeleted_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            var entityId = Guid.Parse("11111111111111111111111111111111");

            evnt = CreateStaticTextDeletedEvent(entityId);

            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new []
            {
                chapter = CreateGroup(children: new[]
                {
                    CreateStaticText(entityId: entityId, text: "static text")
                }),
            });

            var documentStorage = Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocument>>(storage
                => storage.GetById(it.IsAny<string>()) == questionnaire);

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(evnt);

        It should_chapter_be_empty = () =>
            chapter.Children.ShouldBeEmpty();
        
        private static Group chapter;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<StaticTextDeleted> evnt;
    }
}