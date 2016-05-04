using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class InterviewerPlainInterviewFileStorageTestsContext
    {
        public static InterviewerPlainInterviewFileStorage CreateInterviewerPlainInterviewFileStorage(
            IAsyncPlainStorage<InterviewMultimediaView> imageViewStorage = null,
            IAsyncPlainStorage<InterviewFileView> fileViewStorage = null)
        {
            return new InterviewerPlainInterviewFileStorage(
                imageViewStorage: imageViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewMultimediaView>>(),
                fileViewStorage: fileViewStorage ?? Mock.Of<IAsyncPlainStorage<InterviewFileView>>());
        }
    }
}
