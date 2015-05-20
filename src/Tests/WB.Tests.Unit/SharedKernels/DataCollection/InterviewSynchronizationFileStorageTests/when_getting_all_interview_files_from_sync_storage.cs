using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewSynchronizationFileStorageTests
{
    internal class when_getting_all_interview_files_from_sync_storage : InterviewSynchronizationFileStorageTestContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock.Setup(x => x.GetDirectoriesInDirectory(Moq.It.IsAny<string>()))
                .Returns(new[] { interviewId.FormatGuid() });
            fileSystemAccessorMock.Setup(x => x.GetFileName(Moq.It.IsAny<string>())).Returns<string>(name => name);
            fileSystemAccessorMock.Setup(x => x.GetFilesInDirectory(Moq.It.IsAny<string>())).Returns(new[] { fileName1, fileName2 });
            fileSystemAccessorMock.Setup(x => x.ReadAllBytes(fileName1)).Returns(data1);
            fileSystemAccessorMock.Setup(x => x.ReadAllBytes(fileName2)).Returns(data2);
            interviewSynchronizationFileStorage = CreateFileSyncRepository(fileSystemAccessor: fileSystemAccessorMock.Object);
        };

        Because of = () => result = interviewSynchronizationFileStorage.GetBinaryFilesFromSyncFolder();

        It should_return_2_files = () =>
            result.Count.ShouldEqual(2);

        It should_return_first_record_with_data1 = () =>
            result[0].GetData().ShouldEqual(data1);

        It should_return_second_record_with_data2 = () =>
           result[1].GetData().ShouldEqual(data2);

        private static InterviewSynchronizationFileStorage interviewSynchronizationFileStorage;
        private static IList<InterviewBinaryDataDescriptor> result; 
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock = CreateIFileSystemAccessorMock();
        private static Guid interviewId = Guid.NewGuid();
        private static byte[] data1 = new byte[] { 1 };
        private static byte[] data2 = new byte[] { 2 };
        private static string fileName1 = "file1";
        private static string fileName2 = "file2";
    }
}
