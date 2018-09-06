using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.Questionnaire;

namespace WB.UI.WebTester.Infrastructure
{
    class WebTesterTranslationStorage : IWebTesterTranslationService
    {
        private readonly IQuestionnaireTranslator translator;
        private List<TranslationInstance> translationsList;
        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;

        public WebTesterTranslationStorage(IQuestionnaireTranslator translator, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
        {
            this.translator = translator;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
        }

        public void Store(List<TranslationInstance> translations)
        {
            this.translationsList = translations;
        }

        public PlainQuestionnaire Translate(QuestionnaireDocument questionnaire, long version, string language)
        {
            Translation translationId = null;
            QuestionnaireDocument result = questionnaire;

            if (language != null)
            {
                translationId = questionnaire.Translations.SingleOrDefault(t => t.Name == language);

                var translation = translationId != null 
                    ? new QuestionnaireTranslation(translationsList.Where(t => t.TranslationId == translationId.Id)) 
                    : null;

                result = this.translator.Translate(questionnaire, translation);
            }

            return new PlainQuestionnaire(result, version, questionOptionsRepository, substitutionService, translationId);
        }
    }
}
