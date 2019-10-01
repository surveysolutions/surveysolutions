using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;


namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainInterviewFileStorageTests
{
    internal class when_getting_data_for_not_existing_file : ImageQuestionFileStorageTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            imageFileRepository = CreatePlainFileRepository(fileSystemAccessor: FileSystemAccessorMock.Object);
            await BecauseOf();
        }

        public async Task BecauseOf() => result = await imageFileRepository.GetInterviewBinaryData(interviewId, fileName1);

        [NUnit.Framework.Test] public void should_result_Be_equal_to_null () =>
            result.Should().BeNull();

        private static ImageFileStorage imageFileRepository;

        private static readonly Mock<IFileSystemAccessor> FileSystemAccessorMock = CreateIFileSystemAccessorMock();

        private static Guid interviewId = Guid.NewGuid();
        private static string fileName1 = "file1";

        private static byte[] result;
    }
}
