using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
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
            IPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor = null,
            IInterviewerInterviewAccessor interviewFactory = null,
            IPlainStorage<QuestionnaireDocumentView> questionnaireDocuments = null,
            IOptionsRepository optionsRepository = null,
            IPlainStorage<TranslationInstance>  translationRepository = null)
        {
            return new InterviewerQuestionnaireAccessor(
                synchronizationSerializer: synchronizationSerializer ?? Mock.Of<IJsonAllTypesSerializer>(),
                questionnaireViewRepository: questionnaireViewRepository ?? Mock.Of<IPlainStorage<QuestionnaireView>>(),
                questionnaireStorage: questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                questionnaireAssemblyFileAccessor: questionnaireAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                interviewFactory: interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                questionnaireDocuments: questionnaireDocuments ?? Mock.Of< IPlainStorage<QuestionnaireDocumentView>>(),
                optionsRepository: optionsRepository ?? Mock.Of<IOptionsRepository>(),
                translationsStorage: translationRepository ?? Mock.Of<IPlainStorage<TranslationInstance>>());
        }
    }
}
