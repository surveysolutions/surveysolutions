using log4net.Config;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Embedded;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;

namespace WB.Tests.Unit.Infrastructure.Storage
{
    public abstract class RavenDBTestBase
    {
        protected IDocumentStore DocumentStore;

        [SetUp]
        public void SetUpDocumentStore()
        {
            XmlConfigurator.Configure();
            //_documentStore = ConnectToDocumentStore();
            this.DocumentStore = NewDocumentStore();
        }

        [TearDown]
        public void TearDownDocumentStore()
        {
            if (this.DocumentStore != null)
            {
                this.DocumentStore.Dispose();
            }
        }

        private IDocumentStore NewDocumentStore()
        {
            var documentStore = new EmbeddableDocumentStore {
                                        RunInMemory = true
                                    };
            documentStore.UpdateStoreConventions("Snapshots");
            documentStore.Initialize();
            return documentStore;
        }
    }
}