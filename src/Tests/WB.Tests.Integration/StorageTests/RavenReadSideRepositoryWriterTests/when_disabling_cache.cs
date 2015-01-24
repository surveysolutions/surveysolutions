using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Raven.Client;
using Raven.Client.Document;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_disabling_cache : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            documentStore = CreateEmbeddableDocumentStore();
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(ravenStore: documentStore);
            ravenReadSideRepositoryWriter.EnableCache();

            for (int i = 1; i <= cahceLimit+1; i++)
            {
                ravenReadSideRepositoryWriter.Store(new View() { Version = i }, i.ToString());
            }
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.DisableCache();

        It should_store_257_views_to_raven_db = () =>
            documentStore.OpenSession().Query<View>().Count().ShouldEqual(257);

        It should_has_no_cached_items_left = () =>
            ravenReadSideRepositoryWriter.GetReadableStatus().ShouldEqual("cache disabled;    cached: 0;");

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static long cahceLimit = 256;
        private static IDocumentStore documentStore;
    }
}
