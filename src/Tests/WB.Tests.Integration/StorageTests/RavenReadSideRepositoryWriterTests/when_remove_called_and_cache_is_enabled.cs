using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_remove_called_and_cache_is_enabled : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            storedView = new View() { Version = 18 };
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter();
            ravenReadSideRepositoryWriter.EnableCache();
            ravenReadSideRepositoryWriter.Store(storedView, viewId);
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Remove(viewId);

        It should_remove_view_from_repository = () =>
            ravenReadSideRepositoryWriter.GetById(viewId).ShouldBeNull();

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static View storedView;
    }
}
