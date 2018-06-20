using System;
using System.Linq.Expressions;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_storing_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            imageViewStorage.Setup(x => x.Where(Moq.It.IsAny<Expression<Func<InterviewMultimediaView, bool>>>()))
                .Returns(new InterviewMultimediaView[0]);

            interviewerImageFileStorage = CreateInterviewerPlainInterviewFileStorage(
                fileViewStorage: fileViewStorage.Object,
                imageViewStorage: imageViewStorage.Object);
            BecauseOf();
        }

        public void BecauseOf() =>
            interviewerImageFileStorage.StoreInterviewBinaryData(interviewId, imageFileName, imageFileBytes, null);

        [NUnit.Framework.Test] public void should_store_specified_interview_mulimedia_view () =>
            imageViewStorage.Verify(x=>x.Store(Moq.It.IsAny<InterviewMultimediaView>()), Times.Once);

        [NUnit.Framework.Test] public void should_store_specified_interview_file_view () =>
            fileViewStorage.Verify(x=>x.Store(Moq.It.IsAny<InterviewFileView>()), Times.Once);

        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string imageFileName = "image.png";
        private static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private static readonly Mock<IPlainStorage<InterviewMultimediaView>> imageViewStorage = new Mock<IPlainStorage<InterviewMultimediaView>>();
        private static readonly Mock<IPlainStorage<InterviewFileView>> fileViewStorage = new Mock<IPlainStorage<InterviewFileView>>();
        private static InterviewerImageFileStorage interviewerImageFileStorage;
    }
}
