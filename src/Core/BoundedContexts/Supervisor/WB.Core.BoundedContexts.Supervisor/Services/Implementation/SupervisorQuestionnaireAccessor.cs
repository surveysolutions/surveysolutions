using System.Collections.Generic;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorQuestionnaireAccessor : InterviewerQuestionnaireAccessor
    {
        private readonly IPlainStorage<RawQuestionnaireDocumentView> rawQuestionnaireDocuments;

        public SupervisorQuestionnaireAccessor(IJsonAllTypesSerializer synchronizationSerializer, 
            IPlainStorage<QuestionnaireView> questionnaireViewRepository,
            IQuestionnaireStorage questionnaireStorage, 
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor, 
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments,
            IOptionsRepository optionsRepository, 
            IPlainStorage<TranslationInstance> translationsStorage, 
            IPlainStorage<RawQuestionnaireDocumentView> rawQuestionnaireDocuments) 
            : base(synchronizationSerializer, questionnaireViewRepository, questionnaireStorage, questionnaireAssemblyFileAccessor, questionnaireDocuments, optionsRepository, translationsStorage)
        {
            this.rawQuestionnaireDocuments = rawQuestionnaireDocuments;
        }

        public override void StoreQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument, bool census,
            List<TranslationDto> translationDtos)
        {
            this.rawQuestionnaireDocuments.Store(new RawQuestionnaireDocumentView
            {
                Id = questionnaireIdentity.ToString(),
                Document = questionnaireDocument
            });

            base.StoreQuestionnaire(questionnaireIdentity, questionnaireDocument, census, translationDtos);
        }
    }
}
