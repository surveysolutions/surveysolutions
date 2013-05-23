using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using log4net.Config;

namespace Ncqrs.Eventing.Storage.RavenDB.Tests
{
    [TestFixture]
    public class DocumentStoreFailedTests
    {
        protected DocumentStore _documentStore;
        private string path;
        [Test]
        public void Query_IndexWithBool_BoolNotFiltered()
        {
            XmlConfigurator.Configure();
            //_documentStore = ConnectToDocumentStore();
            _documentStore = NewDocumentStore();

            IndexCreation.CreateIndexes(GetType().Assembly,_documentStore);

            var docID = Guid.NewGuid();
            using (var session=_documentStore.OpenSession())
            {
                session.Store(new StoredDoc(docID, true));
                session.SaveChanges();
            }

            using (var readSession=_documentStore.OpenSession())
            {
                var queryWithId = readSession.Query<StoredDoc, TestIndex>().Customize(a=>a.WaitForNonStaleResults()).Where(d => d.MyId == docID);
                Assert.That(queryWithId.Count(),Is.EqualTo(1));

                var queryWithBool = readSession.Query<StoredDoc, TestIndex>().Customize(a => a.WaitForNonStaleResults()).Where(d => d.BoolField);

                Assert.That(queryWithBool.Count(), Is.EqualTo(1));
            }
        }
        private DocumentStore NewDocumentStore()
        {
            path = Path.GetDirectoryName(Assembly.GetAssembly(typeof(RavenDBEventStoreTests)).CodeBase);
            path = Path.Combine(path, "TestRavenDb").Substring(6);
            if (Directory.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Directory);
                Directory.Delete(path, true);
            }
            var documentStore = new EmbeddableDocumentStore
            {
                DataDirectory = path
            };
            documentStore.Initialize();
            return documentStore;
        }
    }
    public class TestIndex : AbstractIndexCreationTask<StoredDoc>
    {
        public TestIndex()
        {
            Map = sEvents => from sEvent in sEvents
                             select
                                 new
                                 {
                                     sEvent.MyId,
                                     sEvent.BoolField
                                 };

            Index(x => x.MyId, FieldIndexing.NotAnalyzed);
            Index(x => x.BoolField, FieldIndexing.Default);
        }
    }
    public class StoredDoc
    {
        public StoredDoc(Guid myId, bool boolField)
        {
            MyId = myId;
            BoolField = boolField;
        }

        public StoredDoc()
        {
        }

        public Guid MyId { get; set; }
        public bool BoolField { get; set; }
    }
}
