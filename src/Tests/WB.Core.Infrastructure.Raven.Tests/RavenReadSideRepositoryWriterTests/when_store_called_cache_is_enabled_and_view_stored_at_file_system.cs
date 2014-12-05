using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_cache_is_enabled_and_view_stored_at_file_system : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            storedView = new View() { Version = 18 };
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);

            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(fileSystemAccessor: fileSystemAccessorMock.Object);
            ravenReadSideRepositoryWriter.EnableCache();
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(storedView, viewId);

        It should_store_view_at_repository = () =>
         ravenReadSideRepositoryWriter.GetById(viewId).ShouldEqual(storedView);

        It should_delete_stored_on_file_system_cache_view = () =>
            fileSystemAccessorMock.Verify(x => x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string viewId = "view id";
        private static View storedView;
    }
}
