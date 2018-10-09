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
    class WebTesterTranslationService : IWebTesterTranslationService
    {
        private readonly IQuestionOptionsRepository questionOptionsRepository;
        private readonly ISubstitutionService substitutionService;
        private readonly IWebTesterTranslationStorage storage;

        public WebTesterTranslationService(IWebTesterTranslationStorage storage, 
            IQuestionOptionsRepository questionOptionsRepository,
            ISubstitutionService substitutionService)
        {
            this.storage = storage;
            this.questionOptionsRepository = questionOptionsRepository;
            this.substitutionService = substitutionService;
        }

        public void Store(List<TranslationInstance> translations)
        {
            this.translationsList = translations;
        }

        public PlainQuestionnaire Translate(QuestionnaireDocument questionnaire, long version, string language)
        {
            QuestionnaireDocument result = storage.GetTranslated(questionnaire, version, language, out Translation translation);
            return new PlainQuestionnaire(result, version, questionOptionsRepository, substitutionService, translation);
        }
    }
}
