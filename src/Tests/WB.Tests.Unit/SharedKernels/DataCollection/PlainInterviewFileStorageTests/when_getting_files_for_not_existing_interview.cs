using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_getting_files_for_not_existing_interview : ImageQuestionFileStorageTestContext
    {
        Establish context = () =>
        {
            imageQuestionFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
        };

        Because of = () => result = imageQuestionFileRepository.GetBinaryFilesForInterview(interviewId);

        It should_0_files_be_returned = () =>
            result.Count.ShouldEqual(0);

        private static ImageQuestionFileStorage imageQuestionFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();

        private static IList<InterviewBinaryDataDescriptor> result;
    }
}
