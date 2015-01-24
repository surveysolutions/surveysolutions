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

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_cache_is_enabled_and_view_stored_at_file_system : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            storedView = new View() { Version = 18 };

            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter();
            ravenReadSideRepositoryWriter.EnableCache();
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(storedView, viewId);

        It should_store_view_at_repository = () =>
         ravenReadSideRepositoryWriter.GetById(viewId).ShouldEqual(storedView);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static View storedView;
    }
}
