using Machine.Specifications;

using Moq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using it = Moq.It;

namespace WB.Tests.Integration.StorageTests.RavenReadSideRepositoryWriterTests
{
    [Subject(typeof(RavenReadSideRepositoryWriter<>))]
    internal class RavenReadSideRepositoryWriterTestsContext
    {
        internal static RavenReadSideRepositoryWriter<View> CreateRavenReadSideRepositoryWriter(
            IDocumentStore ravenStore = null)
        {
            return new RavenReadSideRepositoryWriter<View>(ravenStore ?? CreateEmbeddableDocumentStore(), new RavenReadSideRepositoryWriterSettings(128));
        }

        protected static IDocumentStore CreateEmbeddableDocumentStore()
        {
            IDocumentStore ravenStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };
            ravenStore.Initialize();
            return ravenStore;
        }

        protected static void StoreView(IDocumentStore ravenStore, View viewToStore, string viewId)
        {
            using (var session = ravenStore.OpenSession())
            {
                session.Store(viewToStore, string.Format("View${0}", viewId));
                session.SaveChanges();
            }
        }

        internal class View : IView
        {
            public long Version { get; set; }
        }
    }
}