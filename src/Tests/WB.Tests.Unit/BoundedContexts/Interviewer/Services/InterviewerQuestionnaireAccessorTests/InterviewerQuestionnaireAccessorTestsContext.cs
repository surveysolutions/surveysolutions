using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class InterviewerQuestionnaireAccessorTestsContext
    {
        public static InterviewerQuestionnaireAccessor CreateInterviewerQuestionnaireAccessor(
            IJsonAllTypesSerializer synchronizationSerializer = null,
            IPlainStorage<QuestionnaireView> questionnaireViewRepository = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionnaireAssemblyAccessor questionnaireAssemblyFileAccessor = null,
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments = null,
            IOptionsRepository optionsRepository = null,
            IPlainStorage<TranslationInstance>  translationRepository = null)
        {
            return new InterviewerQuestionnaireAccessor(
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                questionnaireViewRepository: questionnaireViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                questionnaireAssemblyFileAccessor: questionnaireAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyAccessor>(),
                questionnaireDocuments: questionnaireDocuments ?? Mock.Of< IPlainStorage<QuestionnaireDocumentView>>(),
                optionsRepository: optionsRepository ?? Mock.Of<IOptionsRepository>(),
                translationsStorage: translationRepository ?? Mock.Of<IPlainStorage<TranslationInstance>>());
        }
    }
}
