﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewSynchronizationFileStorageTests
{
    internal class when_removing_files_of_not_existing_interview_from_sync_storage : InterviewSynchronizationFileStorageTestContext
    {
        Establish context = () =>
        {
            plainInterviewSynchronizationFileRepository = CreateFileSyncRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () => plainInterviewSynchronizationFileRepository.RemoveInterviewImage(interviewId,"fileName");

        It should_not_put_files_in_sync_storage = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Never);

        private static InterviewSynchronizationFileStorage plainInterviewSynchronizationFileRepository;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
    }
}
