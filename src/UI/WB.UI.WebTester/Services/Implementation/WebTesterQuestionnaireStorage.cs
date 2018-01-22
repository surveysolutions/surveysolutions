using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.UI.WebTester.Services.Implementation
{
    class WebTesterQuestionnaireStorage :  QuestionnaireStorage, IDisposable
    {
        private readonly IDisposable eviction;

        public WebTesterQuestionnaireStorage(
            IPlainKeyValueStorage<QuestionnaireDocument> repository, 
            ITranslationStorage translationStorage, 
            IQuestionnaireTranslator translator,
            IEvictionObservable eviction) : base(repository, translationStorage, translator)
        {
            this.eviction = eviction.Subscribe(token => this.DeleteQuestionnaireDocument(token, 1));
        }

        public void Dispose()
        {
            eviction.Dispose();
        }
    }
}