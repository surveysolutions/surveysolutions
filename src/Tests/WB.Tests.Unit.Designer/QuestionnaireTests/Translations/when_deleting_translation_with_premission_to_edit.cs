using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_deleting_translation_with_premission_to_edit : QuestionnaireTestsContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", ownerId));

            deleteTranslation = Create.Command.DeleteTranslation(questionnaireId, translationId, sharedPersonId);
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.DeleteTranslation(deleteTranslation);

        [Test]
        public void should_doesnt_contain_Translation_with_EntityId_specified() =>
            questionnaire.QuestionnaireDocument.Translations.ShouldNotContain(t => t.Id == translationId);


        private static DeleteTranslation deleteTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Id.gC;
        private static readonly Guid sharedPersonId = Id.gE;
        private static readonly Guid questionnaireId = Id.g1;
        private static readonly Guid translationId = Id.gA;
    }
}