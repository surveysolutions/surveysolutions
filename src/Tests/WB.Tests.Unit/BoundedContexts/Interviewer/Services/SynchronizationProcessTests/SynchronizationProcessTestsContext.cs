using Machine.Specifications;
using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [Subject(typeof(SynchronizationProcess))]
    public class SynchronizationProcessTestsContext
    {
        protected static SynchronizationProcess CreateSynchronizationProcess(IAsyncPlainStorage<InterviewView> interviewViewRepository = null,
            IAsyncPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IAsyncPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IAsyncPlainStorage<InterviewFileView> interviewFileViewStorage = null,
            ISynchronizationService synchronizationService = null,
            ILogger logger = null,
            IUserInteractionService userInteractionService = null,
            IPasswordHasher passwordHasher = null,
            IPrincipal principal = null,
            IMvxMessenger messenger = null,
            IInterviewerQuestionnaireAccessor questionnaireFactory = null,
            IInterviewerInterviewAccessor interviewFactory = null,
            IAttachmentContentStorage attachmentContentStorage = null)
        {
            return new SynchronizationProcess(
                synchronizationService ?? Mock.Of<ISynchronizationService>(),
                interviewersPlainStorage ?? Mock.Of<IAsyncPlainStorage<InterviewerIdentity>>(),
                interviewViewRepository ?? Mock.Of<IAsyncPlainStorage<InterviewView>>(),
                principal ?? Mock.Of<IPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                questionnaireFactory ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                interviewMultimediaViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewFileView>>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>());
        }
    }
}