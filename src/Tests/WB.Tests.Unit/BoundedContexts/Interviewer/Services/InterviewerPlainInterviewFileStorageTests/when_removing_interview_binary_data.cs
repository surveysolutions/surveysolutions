using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_removing_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            fileViewStorage = new SqliteInmemoryStorage<InterviewFileView>();

            fileViewStorage.Store(
                new InterviewFileView
                {
                    Id = imageFileId,
                    File = imageFileBytes
                }
                );

            imageViewStorage = new SqliteInmemoryStorage<InterviewMultimediaView>();
            imageViewStorage.Store(new InterviewMultimediaView
            {
                Id = Guid.NewGuid().FormatGuid(),
                InterviewId = interviewId,
                FileName = imageFileName,
                FileId = imageFileId
            });

            interviewerImageFileStorage = CreateInterviewerPlainInterviewFileStorage(
                fileViewStorage: fileViewStorage,
                imageViewStorage: imageViewStorage);

            BecauseOf();
        }

        public void BecauseOf() =>
            interviewerImageFileStorage.RemoveInterviewBinaryData(interviewId, imageFileName);

        [Test]
        public void should_be_removed_multimedia_views_by_interview_id_and_file_name() =>
            imageViewStorage.Where(x => x.InterviewId == interviewId && x.FileName == imageFileName).Should().BeEmpty();

        [Test]
        public void should_be_removed_file_views_by_interview_id_and_file_name() =>
            fileViewStorage.Where(x => x.Id == imageFileId).Should().BeEmpty();

        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string imageFileName = "image.png";
        private static string imageFileId = "1";
        private static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private static IPlainStorage<InterviewMultimediaView> imageViewStorage;

        private static IPlainStorage<InterviewFileView> fileViewStorage;
        private static InterviewerImageFileStorage interviewerImageFileStorage;
    }
}
