using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_deleting_translation : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", responsibleId));

            deleteTranslation = Create.Command.DeleteTranslation(questionnaireId, translationId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.DeleteTranslation(deleteTranslation);

        It should_raise_TranslationDeleted_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<TranslationDeleted>().TranslationId.ShouldEqual(translationId);

        It should_raise_TranslationDeleted_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<TranslationDeleted>().ResponsibleId.ShouldEqual(responsibleId);

        private static DeleteTranslation deleteTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}