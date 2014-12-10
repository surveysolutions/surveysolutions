using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.Storage.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_cache_is_enabled_and_cache_limit_is_reached : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            fileSystemAccessorMock = new Mock<IFileSystemAccessor>();

            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(fileSystemAccessor: fileSystemAccessorMock.Object);
            ravenReadSideRepositoryWriter.EnableCache();

            for (int i = 1; i <= cahceLimit; i++)
            {
                ravenReadSideRepositoryWriter.Store(new View() { Version = i }, i.ToString());
            }
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(new View() { Version = cahceLimit+1 }, viewId);

        It should_move_16_cached_items_from_memory_cache_to_file_system_cache = () =>
            fileSystemAccessorMock.Verify(x => x.WriteAllText(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()), Times.Exactly(16));

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static Mock<IFileSystemAccessor> fileSystemAccessorMock;
        private static string viewId = "view id";
        private static long cahceLimit = 256;
    }
}
