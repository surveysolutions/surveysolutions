using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Raven.Client.Document;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    internal class when_disabling_cache : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            fileSystemAccessor = new FileSystemIOAccessor();

            documentStore = CreateEmbeddableDocumentStore();
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(fileSystemAccessor: fileSystemAccessor, ravenStore: documentStore);
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
            ravenReadSideRepositoryWriter.GetReadableStatus().ShouldEqual("cache disabled;    cached:   0;    not stored:   0");

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static FileSystemIOAccessor fileSystemAccessor;
        private static long cahceLimit = 256;
        private static DocumentStore documentStore;
    }
}
