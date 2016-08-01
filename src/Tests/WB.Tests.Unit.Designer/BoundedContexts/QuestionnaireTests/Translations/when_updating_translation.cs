using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire.Translation;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_updating_translation : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", responsibleId));

            updateTranslation = Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, name, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => questionnaire.AddOrUpdateTranslation(updateTranslation);

        It should_raise_TranslationUpdated_event_with_EntityId_specified = () =>
            eventContext.GetSingleEvent<TranslationUpdated>().TranslationId.ShouldEqual(translationId);

        It should_raise_TranslationUpdated_event_with_ResponsibleId_specified = () =>
            eventContext.GetSingleEvent<TranslationUpdated>().ResponsibleId.ShouldEqual(responsibleId);

        It should_raise_TranslationUpdated_event_with_TranslationName_specified = () =>
            eventContext.GetSingleEvent<TranslationUpdated>().Name.ShouldEqual(name);

        private static AddOrUpdateTranslation updateTranslation;
        private static Questionnaire questionnaire;
        private static readonly string name = "translation";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}