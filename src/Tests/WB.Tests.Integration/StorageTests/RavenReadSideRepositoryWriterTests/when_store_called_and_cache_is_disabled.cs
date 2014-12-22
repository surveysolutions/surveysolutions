using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Raven.Client.Document;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    internal class when_store_called_and_cache_is_disabled : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            documentStore = CreateEmbeddableDocumentStore();
            storedView = new View() { Version = 18 };
            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(ravenStore: documentStore);
        };

        Because of = () =>
            ravenReadSideRepositoryWriter.Store(storedView,viewId);

        It should_store_item_at_repository = () =>
            ravenReadSideRepositoryWriter.GetById(viewId).Version.ShouldEqual(18);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static string viewId = "view id";
        private static View storedView;
        private static DocumentStore documentStore;
    }
}
