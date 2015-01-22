using Machine.Specifications;
using Raven.Client;
using Raven.Client.Document;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_and_cache_is_disabled : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            var documentStore = CreateEmbeddableDocumentStore();
            storedView = new View() { Version = 18 };
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(ravenStore: documentStore);
            session = documentStore.OpenSession();
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(storedView,viewId);

        It should_store_item_at_repository = () =>
            session.Load<View>(string.Format("View${0}", viewId)).Version.ShouldEqual(18);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static View storedView;
        private static IDocumentSession session;
    }
}
