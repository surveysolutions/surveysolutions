using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_deleting_translation_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", ownerId));

            deleteTranslation = Create.Command.DeleteTranslation(questionnaireId, translationId, sharedPersonId);
        };


        Because of = () => questionnaire.DeleteTranslation(deleteTranslation);

        It should_doesnt_contain_Translation_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Translations.ShouldNotContain(t => t.Id == translationId);


        private static DeleteTranslation deleteTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}