using Machine.Specifications;

using Moq;

using Raven.Client.Document;
using Raven.Client.Embedded;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using it = Moq.It;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryWriterTests
{
    [Subject(typeof(RavenReadSideRepositoryWriter<>))]
    internal class RavenReadSideRepositoryWriterTestsContext
    {
        internal static RavenReadSideRepositoryWriter<View> CreateRavenReadSideRepositoryWriter(
            DocumentStore ravenStore = null, IFileSystemAccessor fileSystemAccessor=null)
        {
            return new RavenReadSideRepositoryWriter<View>(
                ravenStore ?? CreateEmbeddableDocumentStore(), fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(), "");
        }

        protected static DocumentStore CreateEmbeddableDocumentStore()
        {
            var ravenStore = new EmbeddableDocumentStore
            {
                RunInMemory = true
            };
            ravenStore.Initialize();
            return ravenStore;
        }

        protected static void StoreView(DocumentStore ravenStore, View viewToStore, string viewId)
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