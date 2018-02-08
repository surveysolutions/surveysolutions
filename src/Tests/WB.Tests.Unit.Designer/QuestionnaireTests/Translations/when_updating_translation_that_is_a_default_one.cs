using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_updating_translation_that_is_a_default_one : QuestionnaireTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, TranslationId, "", ownerId));
            questionnaire.SetDefaultTranslation(Create.Command.SetDefaultTranslation(questionnaireId, TranslationId, ownerId));
        }

        private void BecauseOf() =>
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, NewTranslationId, "", ownerId, TranslationId));

        
        [Test]
        public void should_update_default_translation()
        {
            BecauseOf();
            Assert.That(questionnaire.QuestionnaireDocument.DefaultTranslation, Is.EqualTo(NewTranslationId));
        }
        
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Id.gC;
        private static readonly Guid sharedPersonId = Id.gE;
        private static readonly Guid questionnaireId = Id.g1;
        private static readonly Guid TranslationId = Id.gA;

        private static readonly Guid NewTranslationId = Id.gB;
    }
}