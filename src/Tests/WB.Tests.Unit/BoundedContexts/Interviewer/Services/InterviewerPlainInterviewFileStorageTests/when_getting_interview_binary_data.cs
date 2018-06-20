using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Implementation.Repositories;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_getting_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var imageViewStorage = new SqliteInmemoryStorage<InterviewMultimediaView>();
            imageViewStorage.Store(
                new InterviewMultimediaView
                {
                    Id = Guid.NewGuid().FormatGuid(),
                    InterviewId = interviewId,
                    FileName = imageFileName,
                    FileId = imageFileId
                });

            var fileViewStorage = new SqliteInmemoryStorage<InterviewFileView>();
            fileViewStorage.Store(
                new InterviewFileView
                {
                    Id = imageFileId,
                    File = imageFileBytes
                });

            interviewerImageFileStorage = CreateInterviewerPlainInterviewFileStorage(
                fileViewStorage: fileViewStorage,
                imageViewStorage: imageViewStorage);
            BecauseOf();
        }

        public void BecauseOf() =>
            bytesResult = interviewerImageFileStorage.GetInterviewBinaryData(interviewId, imageFileName);

        [Test]
        public void should_remove_questionnaire_document_view_from_plain_storage() =>
            bytesResult.Should().BeEquivalentTo(imageFileBytes);
        
        static byte[] bytesResult;
        static InterviewerImageFileStorage interviewerImageFileStorage;
        static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        static string imageFileName = "image.png";
        static string imageFileId = "1";
        static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
    }
}
