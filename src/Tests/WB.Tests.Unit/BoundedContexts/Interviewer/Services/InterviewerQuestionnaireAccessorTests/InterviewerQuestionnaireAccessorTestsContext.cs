using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerQuestionnaireAccessorTests
{
    internal class InterviewerQuestionnaireAccessorTestsContext
    {
        public static InterviewerQuestionnaireAccessor CreateInterviewerQuestionnaireAccessor(
            ISerializer serializer = null,
            IQuestionnaireModelBuilder questionnaireModelBuilder = null,
            IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository = null,
            IAsyncPlainStorage<QuestionnaireView> questionnaireViewRepository = null,
            IAsyncPlainStorage<QuestionnaireDocumentView> questionnaireDocumentRepository = null,
            IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor = null,
            IInterviewerInterviewAccessor interviewFactory = null)
        {
            return new InterviewerQuestionnaireAccessor(
                serializer: serializer ?? Mock.Of<ISerializer>(),
                questionnaireModelBuilder: questionnaireModelBuilder ?? Mock.Of<IQuestionnaireModelBuilder>(),
                questionnaireModelViewRepository: questionnaireModelViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireModelView>>(),
                questionnaireViewRepository: questionnaireViewRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireView>>(),
                questionnaireDocumentRepository: questionnaireDocumentRepository ?? Mock.Of<IAsyncPlainStorage<QuestionnaireDocumentView>>(),
                interviewViewRepository: interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                questionnaireAssemblyFileAccessor: questionnaireAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                interviewFactory: interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>());
        }
    }
}
