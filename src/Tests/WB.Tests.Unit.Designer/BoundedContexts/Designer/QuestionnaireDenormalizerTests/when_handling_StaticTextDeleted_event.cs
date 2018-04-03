using System;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;
using Group = Main.Core.Entities.SubEntities.Group;

using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextDeleted_event : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            entityId = Guid.Parse("11111111111111111111111111111111");
            
            QuestionnaireDocument questionnaire = CreateQuestionnaireDocument(children: new []
            {
                chapter = CreateGroup(children: new[]
                {
                    Create.StaticText(staticTextId: entityId, text: "static text")
                }),
            },createdBy:responsibleId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            BecauseOf();
        }

        private void BecauseOf() =>
            denormalizer.DeleteStaticText(entityId, responsibleId.Value);

        [NUnit.Framework.Test] public void should_chapter_be_empty () =>
            chapter.Children.Should().BeEmpty();
        
        private static Group chapter;
        private static Questionnaire denormalizer;
        private static Guid entityId;
        private  static Guid? responsibleId = Guid.Parse("33111111111111111111111111111111");
    }
}
