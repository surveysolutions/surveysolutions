using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    internal class when_get_by_id_called_and_cache_is_enabled : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            storedView = new View(){Version = 18};
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter();
            ravenReadSideRepositoryWriter.EnableCache();
            ravenReadSideRepositoryWriter.Store(storedView, viewId);
        };

        Because of = () =>
            result = ravenReadSideRepositoryWriter.GetById(viewId);

        It should_return_stored_in_cache_view = () =>
            result.Version.ShouldEqual(18);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static View result;
        private static View storedView;
    }
}
