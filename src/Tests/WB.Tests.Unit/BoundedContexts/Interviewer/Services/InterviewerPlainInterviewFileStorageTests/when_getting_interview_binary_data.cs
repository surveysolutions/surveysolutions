using System;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_getting_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        Establish context = () =>
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
        };

        Because of = () =>
            bytesResult = interviewerImageFileStorage.GetInterviewBinaryData(interviewId, imageFileName);

        It should_remove_questionnaire_document_view_from_plain_storage = () =>
            bytesResult.ShouldAllBeEquivalentTo(imageFileBytes);
        
        static byte[] bytesResult;
        static InterviewerImageFileStorage interviewerImageFileStorage;
        static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        static string imageFileName = "image.png";
        static string imageFileId = "1";
        static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
    }
}
