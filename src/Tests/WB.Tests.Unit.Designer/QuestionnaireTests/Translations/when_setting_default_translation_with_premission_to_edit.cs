using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_setting_default_translation_with_premission_to_edit : QuestionnaireTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, TranslationId, "", ownerId));
        }

        private void BecauseOf(Guid? translationId) => 
            questionnaire.SetDefaultTranslation(Create.Command.SetDefaultTranslation(questionnaireId, translationId, sharedPersonId));

        [Test]
        public void should_set_default_translation()
        {
            BecauseOf(TranslationId);
            Assert.That(questionnaire.QuestionnaireDocument.DefaultTranslation, Is.EqualTo(TranslationId));
        }

        [Test]
        public void should_clear_default_translation()
        {
            BecauseOf(null);
            Assert.That(questionnaire.QuestionnaireDocument.DefaultTranslation, Is.EqualTo(null));
        }
        
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Id.gC;
        private static readonly Guid sharedPersonId = Id.gE;
        private static readonly Guid questionnaireId = Id.g1;
        private static readonly Guid TranslationId = Id.gA;
    }
}