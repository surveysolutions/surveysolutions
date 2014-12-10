using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Raven.Client.Extensions;
using Raven.Imports.Newtonsoft.Json;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.Storage.RavenReadSideRepositoryWriterTests
{
    internal class when_get_by_id_called_cache_is_enabled_and_view_stored_at_file_system : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            storedView = new View(){Version = 18};
            fileSystemAccessorMock=new Mock<IFileSystemAccessor>();
            fileSystemAccessorMock.Setup(x => x.IsFileExists(Moq.It.IsAny<string>())).Returns(true);
            fileSystemAccessorMock.Setup(x => x.ReadAllText(Moq.It.IsAny<string>())).Returns(JsonConvert.SerializeObject(storedView));

            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(fileSystemAccessor: fileSystemAccessorMock.Object);
            ravenReadSideRepositoryWriter.EnableCache();
        };

        Because of = () =>
            result = ravenReadSideRepositoryWriter.GetById(viewId);

        It should_return_stored_in_file_system_cache_view = () =>
            result.Version.ShouldEqual(18);

        It should_delete_stored_on_file_system_cache_view = () =>
            fileSystemAccessorMock.Verify(x=>x.DeleteFile(Moq.It.IsAny<string>()), Times.Once);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string viewId = "view id";
        private static View result;
        private static View storedView;
    }
}
