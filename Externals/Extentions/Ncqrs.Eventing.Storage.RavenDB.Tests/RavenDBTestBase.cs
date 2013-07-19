using System.IO;
using System.Reflection;
using log4net.Config;
using NUnit.Framework;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace Ncqrs.Eventing.Storage.RavenDB.Tests
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
                                        Conventions = new DocumentConvention
                                            {
                                                JsonContractResolver = new PropertiesOnlyContractResolver(),
                                                FindTypeTagName = x => "Snapshots"
                                                /*, CustomizeJsonSerializer = serializer => serializer.Binder = new TypeNameSerializationBinder("{0}");*/
                                            }
                                    };
            documentStore.Initialize();
            return documentStore;
        }
    }
}