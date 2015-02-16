using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Raven.Client;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    internal class when_bulk_store_used : RavenReadSideRepositoryWriterTestsContext
    {
        Establish context = () =>
        {
            var documentStore = CreateEmbeddableDocumentStore();
            itemsToStore = new List<Tuple<View, string>>();
            for (int i = 0; i < 10; i++)
            {
                var storedView = new View() { Version = i };
                viewId = "ViewId";
                itemsToStore.Add(Tuple.Create(storedView, viewId + i));
            }

            ravenReadSideRepositoryWriter = CreateRavenReadSideRepositoryWriter(ravenStore: documentStore);
            session = documentStore.OpenSession();
        };

        Because of = () => ravenReadSideRepositoryWriter.BulkStore(itemsToStore);

        It should_store_all_items_at_repository = () => session.Query<View>().ToList().Count.ShouldEqual(10);

        It should_use_raven_ids_to_store_entity = () => session.Load<View>(string.Format("View${0}1", viewId)).Version.ShouldEqual(1);

        private static RavenReadSideRepositoryWriter<View> ravenReadSideRepositoryWriter;
        private static IDocumentSession session;
        private static List<Tuple<View, string>> itemsToStore;
        private static string viewId;
    }
}