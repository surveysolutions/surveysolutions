using System;
using FluentAssertions;
using Machine.Specifications;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_getting_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        Establish context = () =>
        {
            var imageViewStorage = new TestAsyncPlainStorage<InterviewMultimediaView>(new[]
            {
                new InterviewMultimediaView
                {
                    InterviewId = interviewId,
                    FileName = imageFileName,
                    FileId = imageFileId
                }
            });

            var fileViewStorage = new TestAsyncPlainStorage<InterviewFileView>(new[]
            {
                new InterviewFileView
                {
                    Id = imageFileId,
                    File = imageFileBytes
                }
            });

            interviewerPlainInterviewFileStorage = CreateInterviewerPlainInterviewFileStorage(
                fileViewStorage: fileViewStorage,
                imageViewStorage: imageViewStorage);
        };

        Because of = () =>
            bytesResult = interviewerPlainInterviewFileStorage.GetInterviewBinaryData(interviewId, imageFileName);

        It should_remove_questionnaire_document_view_from_plain_storage = () =>
            bytesResult.ShouldAllBeEquivalentTo(imageFileBytes);
        
        private static byte[] bytesResult;
        private static InterviewerPlainInterviewFileStorage interviewerPlainInterviewFileStorage;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string imageFileName = "image.png";
        private static string imageFileId = "1";
        private static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0};
    }
}
