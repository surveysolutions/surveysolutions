using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_adding_or_updating_translation_with_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            addOrUpdateTranslation = Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", sharedPersonId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);
            BecauseOf();
        }


        private void BecauseOf() => questionnaire.AddOrUpdateTranslation(addOrUpdateTranslation);

        [NUnit.Framework.Test] public void should_contains_Translation_with_EntityId_specified () =>
            questionnaire.QuestionnaireDocument.Translations.Should().Contain(t => t.Id == translationId);

        private static AddOrUpdateTranslation addOrUpdateTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
