using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class InterviewerPlainInterviewFileStorageTestsContext
    {
        public static InterviewerImageFileStorage CreateInterviewerPlainInterviewFileStorage(
            IPlainStorage<InterviewMultimediaView> imageViewStorage = null,
            IPlainStorage<InterviewFileView> fileViewStorage = null,
            IEncryptionService encryptionService = null)
        {
            if (encryptionService == null)
            {
                var mockOfEncryptionService = new Mock<IEncryptionService>();
                mockOfEncryptionService.Setup(x => x.Decrypt(It.IsAny<byte[]>())).Returns<byte[]>(c => c);
                encryptionService = mockOfEncryptionService.Object;
            }

            return new InterviewerImageFileStorage(
                imageViewStorage: imageViewStorage ?? Mock.Of<IPlainStorage<InterviewMultimediaView>>(),
                fileViewStorage: fileViewStorage ?? Mock.Of<IPlainStorage<InterviewFileView>>(),
                encryptionService: encryptionService);
        }
    }
}
