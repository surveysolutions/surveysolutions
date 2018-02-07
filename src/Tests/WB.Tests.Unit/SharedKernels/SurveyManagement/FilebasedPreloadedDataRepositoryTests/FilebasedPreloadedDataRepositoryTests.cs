using System;
using System.IO;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.FilebasedPreloadedDataRepositoryTests
{
    [TestFixture]
    [TestOf(typeof(FilebasedPreloadedDataRepository))]
    internal class FilebasedPreloadedDataRepositoryTests : FilebasedPreloadedDataRepositoryTestContext
    {
        [Test]
        public void When_deleting_preloaded_data_and_there_is_old_session_folder_Then_should_delete_it()
        {
            // arrange
            var fileAccessorMock = Mock.Get(Mock.Of<IFileSystemAccessor>(_
                => _.GetDirectoriesInDirectory(@"x:\path") == new[] { @"x:\path\old", @"x:\path\new" }
                && _.CombinePath(@"x:\", It.IsAny<string>()) == @"x:\path"
                && _.CombinePath(@"x:\path", It.Is<string>(s => IsGuid(s))) == @"x:\path\new"
                && _.CombinePath(@"x:\path", "current") == @"x:\path\current"
                && _.IsDirectoryExists(@"x:\path\current")
                && _.OpenOrCreateFile(It.IsAny<string>(), It.IsAny<bool>()) == new MemoryStream(new byte[] { })));

            fileAccessorMock
                .Setup(_ => _.GetFileName(It.IsAny<string>()))
                .Returns<string>(path => Path.GetFileName(path));

            var preloadedDataRepository = CreateFilebasedPreloadedDataRepository(
                folderPath: @"x:\",
                fileSystemAccessor: fileAccessorMock.Object);

            var newFolder = preloadedDataRepository.Store(new MemoryStream(new byte[] { }), "new.stream");

            fileAccessorMock
                .Setup(_ => _.GetDirectoriesInDirectory(@"x:\path"))
                .Returns(new[] { @"x:\path\old", $@"x:\path\{newFolder}" });

            // act
            preloadedDataRepository.DeletePreloadedData("current");

            // assert
            fileAccessorMock.Verify(x => x.DeleteDirectory(@"x:\path\old"), Times.Once);
            fileAccessorMock.Verify(x => x.DeleteDirectory(@"x:\path\current"), Times.Once);
            fileAccessorMock.Verify(x => x.DeleteDirectory($@"x:\path\{newFolder}"), Times.Never);
        }

        private static bool IsGuid(string s)
        {
            Guid _;
            return Guid.TryParse(s, out _);
        }
    }
}