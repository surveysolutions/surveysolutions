using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.PlainInterviewFileStorageTests
{
    internal class when_getting_files_for_not_existing_interview : PlainInterviewFileStorageTestContext
    {
        Establish context = () =>
        {
            plainFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => result = plainFileRepository.GetBinaryFilesForInterview(interviewId);

        It should_0_files_be_returned = () =>
            result.Count.ShouldEqual(0);

        private static PlainInterviewFileStorage plainFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();

        private static IList<InterviewBinaryDataDescriptor> result;
    }
}
