using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_adding_or_updating_translation_with_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            addOrUpdateTranslation = Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", sharedPersonId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.Edit, ownerId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddOrUpdateTranslation(addOrUpdateTranslation);

        It should_raise_TranslationUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<TranslationUpdated>().TranslationId.ShouldEqual(translationId);

        It should_raise_TranslationUpdated_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<TranslationUpdated>().ResponsibleId.ShouldEqual(sharedPersonId);

        private static AddOrUpdateTranslation addOrUpdateTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}