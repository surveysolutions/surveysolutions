using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Raven.Client;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_cache_is_enabled_and_cache_limit_is_reached : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            var documentStore = CreateEmbeddableDocumentStore();
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(ravenStore: documentStore);
            ravenReadSideRepositoryWriter.EnableCache();

            for (int i = 1; i <= cahceLimit; i++)
            {
                ravenReadSideRepositoryWriter.Store(new View() { Version = i }, i.ToString());
            }
            session = documentStore.OpenSession();
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(new View() { Version = cahceLimit+1 }, viewId);

        Cleanup stuff = () =>
        {
            session.Dispose();
        };

        It should_move_16_cached_items_from_memory_cache_to_database= () =>
            session.Query<View>().Count().ShouldEqual(16);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static long cahceLimit = 256;
        private static IDocumentSession session;
    }
}
