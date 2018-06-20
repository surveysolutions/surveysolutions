using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class InterviewerPlainInterviewFileStorageTestsContext
    {
        public static InterviewerImageFileStorage CreateInterviewerPlainInterviewFileStorage(
            IPlainStorage<InterviewMultimediaView> imageViewStorage = null,
            IPlainStorage<InterviewFileView> fileViewStorage = null)
        {
            return new InterviewerImageFileStorage(
                imageViewStorage: imageViewStorage ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                fileViewStorage: fileViewStorage ?? Mock.Of<IPlainStorage<InterviewFileView>>());
        }
    }
}
