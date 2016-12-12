using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_updating_translation : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, oldTranslationId, "", responsibleId));

            updateTranslation = Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, name, responsibleId, oldTranslationId);
        };


        Because of = () => questionnaire.AddOrUpdateTranslation(updateTranslation);

        It should_contains_Translation_with_EntityId_specified = () =>
            questionnaire.QuestionnaireDocument.Translations.ShouldContain(t => t.Id == translationId);

        It should_contains_Translation_with_TranslationName_specified = () =>
            questionnaire.QuestionnaireDocument.Translations.Single(t => t.Id == translationId).Name.ShouldEqual(name);

        private static AddOrUpdateTranslation updateTranslation;
        private static Questionnaire questionnaire;
        private static readonly string name = "translation";
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid oldTranslationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}