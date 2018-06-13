using System;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_AddingMacro : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocument(questionnaireId, userId: creatorId);
            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaire);
            command = new AddMacro(questionnaire.PublicKey, entityId, creatorId);
            BecauseOf();
        }

        private void BecauseOf() => denormalizer.AddMacro(command);

        [NUnit.Framework.Test] public void should_add_one_macro () =>
            questionnaire.Macros.Count.Should().Be(1);

        [NUnit.Framework.Test] public void should_add_macro_with_key_equals_entity_id () =>
           questionnaire.Macros.ContainsKey(entityId).Should().BeTrue();

        [NUnit.Framework.Test] public void should_add_macro_with_empty_name () =>
           questionnaire.Macros[entityId].Name.Should().BeNull();

        [NUnit.Framework.Test] public void should_add_macro_with_empty_content () =>
           questionnaire.Macros[entityId].Content.Should().BeNull();

        [NUnit.Framework.Test] public void should_add_macro_with_empty_description () =>
           questionnaire.Macros[entityId].Description.Should().BeNull();

        private static Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid creatorId = Guid.Parse("11111111111111111111111111111112");
        private static Questionnaire denormalizer;
        private static QuestionnaireDocument questionnaire;
        private static AddMacro command;
    }
}