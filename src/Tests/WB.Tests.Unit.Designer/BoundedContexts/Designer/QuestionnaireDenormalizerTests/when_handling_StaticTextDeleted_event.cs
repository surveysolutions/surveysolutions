using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.GenericSubdomains.Portable;
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
            entityId = Guid.Parse("11111111111111111111111111111111");
            
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new []
            {
                chapter = CreateGroup(children: new[]
                {
                    Create.StaticText(staticTextId: entityId, text: "static text")
                }),
            },createdBy:responsibleId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
        };

        Because of = () =>
            denormalizer.DeleteStaticText(entityId, responsibleId.Value);

        It should_chapter_be_empty = () =>
            chapter.Children.ShouldBeEmpty();
        
        private static Group chapter;
        private static Questionnaire denormalizer;
        private static Guid entityId;
        private  static Guid? responsibleId = Guid.Parse("33111111111111111111111111111111");
    }
}