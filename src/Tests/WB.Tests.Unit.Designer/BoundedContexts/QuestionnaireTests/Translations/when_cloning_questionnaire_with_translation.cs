using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_cloning_questionnaire_with_translation : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            sourceQuestionnaire = Create.QuestionnaireDocument();
            sourceQuestionnaire.Translations.Add(Create.Translation(translationId: translationId, name: name));
        };


        Because of = () => 
            questionnaire.CloneQuestionnaire("Title", false, responsibleId, clonedQuestionnaireId, sourceQuestionnaire);


        It should_raise_QuestionnaireCloned_event_with_1_attachment = () =>
            questionnaire.QuestionnaireDocument.Translations.Count.ShouldEqual(1);

        It should_set_new_TranslationId_in_raised_event = () =>
            questionnaire.QuestionnaireDocument.Translations.First().Id.ShouldNotEqual(translationId);

        It should_set_original_Name_in_raised_event = () =>
            questionnaire.QuestionnaireDocument.Translations.First().Name.ShouldEqual(name);


        private static Questionnaire questionnaire;
        private static QuestionnaireDocument sourceQuestionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid clonedQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string name = "name";
    }
}