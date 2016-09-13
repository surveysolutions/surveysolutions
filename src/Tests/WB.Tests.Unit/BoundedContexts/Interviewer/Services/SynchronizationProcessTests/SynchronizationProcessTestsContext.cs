using Machine.Specifications;
using Moq;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [Subject(typeof(SynchronizationProcess))]
    public class SynchronizationProcessTestsContext
    {
        protected static SynchronizationProcess CreateSynchronizationProcess(IPlainStorage<InterviewView> interviewViewRepository = null,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage = null,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage = null,
            IPlainStorage<InterviewFileView> interviewFileViewStorage = null,
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
                interviewersPlainStorage ?? Mock.Of<IPlainStorage<InterviewerIdentity>>(),
                interviewViewRepository ?? Mock.Of<IPlainStorage<InterviewView>>(),
                principal ?? Mock.Of<IPrincipal>(),
                logger ?? Mock.Of<ILogger>(),
                userInteractionService ?? Mock.Of<IUserInteractionService>(),
                questionnaireFactory ?? Mock.Of<IInterviewerQuestionnaireAccessor>(),
                attachmentContentStorage ?? Mock.Of<IAttachmentContentStorage>(),
                interviewFactory ?? Mock.Of<IInterviewerInterviewAccessor>(),
                interviewMultimediaViewStorage ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                interviewFileViewStorage ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                Mock.Of<AttachmentsCleanupService>(),
                passwordHasher ?? Mock.Of<IPasswordHasher>());
        }
    }
}