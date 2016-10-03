using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using Group = Main.Core.Entities.SubEntities.Group;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
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
                    Create.StaticText(staticTextId: entityId, text: "static text")
                }),
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.DeleteStaticText(evnt);

        It should_chapter_be_empty = () =>
            chapter.Children.ShouldBeEmpty();
        
        private static Group chapter;
        private static Questionnaire denormalizer;
        private static StaticTextDeleted evnt;
    }
}