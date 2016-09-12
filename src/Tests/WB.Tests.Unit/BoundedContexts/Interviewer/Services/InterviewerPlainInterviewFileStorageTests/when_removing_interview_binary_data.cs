using System;
using Machine.Specifications;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.InterviewerPlainInterviewFileStorageTests
{
    internal class when_removing_interview_binary_data : InterviewerPlainInterviewFileStorageTestsContext
    {
        Establish context = () =>
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

            interviewerPlainInterviewFileStorage = CreateInterviewerPlainInterviewFileStorage(
                fileViewStorage: fileViewStorage,
                imageViewStorage: imageViewStorage);
        };

        Because of = () =>
            interviewerPlainInterviewFileStorage.RemoveInterviewBinaryData(interviewId, imageFileName);

        It should_be_removed_multimedia_views_by_interview_id_and_file_name = () =>
            imageViewStorage.Where(x => x.InterviewId == interviewId && x.FileName == imageFileName).ShouldBeEmpty();

        It should_be_removed_file_views_by_interview_id_and_file_name = () =>
            fileViewStorage.Where(x => x.Id == imageFileId).ShouldBeEmpty();

        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static string imageFileName = "image.png";
        private static string imageFileId = "1";
        private static readonly byte[] imageFileBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
        private static IAsyncPlainStorage<InterviewMultimediaView> imageViewStorage;

        private static IAsyncPlainStorage<InterviewFileView> fileViewStorage;
        private static InterviewerPlainInterviewFileStorage interviewerPlainInterviewFileStorage;
    }
}
