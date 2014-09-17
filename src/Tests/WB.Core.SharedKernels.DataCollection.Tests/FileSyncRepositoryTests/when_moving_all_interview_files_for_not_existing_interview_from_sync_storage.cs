using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Tests.PlainFileRepositoryTests;

namespace WB.Core.SharedKernels.DataCollection.Tests.FileSyncRepositoryTests
{
    internal class when_moving_all_interview_files_for_not_existing_interview_from_sync_storage : FileSyncRepositoryTestContext
    {
        Establish context = () =>
        {
            plainFileRepository = CreateFileSyncRepository();
        };

        Because of = () => plainFileRepository.RemoveBinaryDataFromSyncFolder(interviewId,"fileName");

        It should_sync_storage_contains_0_files = () =>
            plainFileRepository.GetBinaryFilesFromSyncFolder().Count.ShouldEqual(0);

        private static FileSyncRepository plainFileRepository;
        private static Guid interviewId = Guid.NewGuid();
    }
}
