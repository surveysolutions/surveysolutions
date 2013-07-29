using System.IO;
using System.Reflection;
using log4net.Config;
using Ncqrs.Eventing.Storage.RavenDB;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace WB.Core.Infrastructure.Raven.Tests
{
    public abstract class RavenDBTestBase
    {
        protected DocumentStore _documentStore;

        [SetUp]
        public void SetUpDocumentStore()
        {
            XmlConfigurator.Configure();
            //_documentStore = ConnectToDocumentStore();
            _documentStore = NewDocumentStore();
        }

        [TearDown]
        public void TearDownDocumentStore()
        {
            if (_documentStore != null)
            {
                _documentStore.Dispose();
            }
        }

        private DocumentStore NewDocumentStore()
        {
            var documentStore = new EmbeddableDocumentStore
                                    {
                                        RunInMemory = true,
                                        Conventions = RavenWriteSideStore.CreateStoreConventions("Snapshots"),
                                    };
            documentStore.Initialize();
            return documentStore;
        }
    }
}