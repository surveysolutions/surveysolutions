using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.FileSyncRepositoryTests
{
    internal class when_moving_all_interview_files_for_not_existing_interview : FileSyncRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepositoryMock.Setup(x => x.GetBinaryFilesForInterview(interviewId))
             .Returns(new List<InterviewBinaryDataDescriptor>());
            interviewSynchronizationFileStorage = CreateFileSyncRepository(plainFileRepository: plainFileRepositoryMock.Object);
        };

        Because of = () => interviewSynchronizationFileStorage.MoveInterviewsBinaryDataToSyncFolder(interviewId);

        It should_sync_storage_contains_0_files = () =>
            interviewSynchronizationFileStorage.GetBinaryFilesFromSyncFolder().Count.ShouldEqual(0);

        private static InterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private static Mock<IPlainInterviewFileStorage> plainFileRepositoryMock = new Mock<IPlainInterviewFileStorage>();
        private static Guid interviewId = Guid.NewGuid();
    }
}
