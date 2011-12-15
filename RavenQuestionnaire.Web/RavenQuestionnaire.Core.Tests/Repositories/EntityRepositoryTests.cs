using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Raven.Client;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.Repositories
{
    [TestFixture]
    public class EntityRepositoryTests
    {
        [Test]
        public void Add_SavesDocumentToSession()
        {
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            DummyRepository repository = new DummyRepository(documentSessionMock.Object);
            DummyDocument innerDoc = new DummyDocument();
            DummyEntity entity = new DummyEntity(innerDoc);
            repository.Add(entity);

            documentSessionMock.Verify(x => x.Store(innerDoc), Times.Once());

        }

        [Test]
        public void Remove_DeletesDocumentFromSession()
        {
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            DummyRepository repository = new DummyRepository(documentSessionMock.Object);
            DummyDocument innerDoc = new DummyDocument();
            DummyEntity entity = new DummyEntity(innerDoc);
            repository.Remove(entity);

            documentSessionMock.Verify(x => x.Delete(innerDoc), Times.Once());
        }

        [Test]
        public void Load_LoadsDocumentFromSession()
        {
            Mock<IDocumentSession> documentSessionMock = new Mock<IDocumentSession>();
            DummyRepository repository = new DummyRepository(documentSessionMock.Object);
            DummyDocument innerDoc = new DummyDocument();

            documentSessionMock.Setup(x => x.Load<DummyDocument>("testId")).Returns(innerDoc);

            DummyEntity entity = repository.Load("testId");
            Assert.AreEqual(innerDoc, entity.GetInnerDocument());
        }

        private class DummyDocument
        {
        }

        private class DummyEntity : IEntity<DummyDocument>
        {
            private DummyDocument dummyDocument;

            public DummyEntity(DummyDocument innerDocument)
            {
                this.dummyDocument = innerDocument;
            }

            public DummyDocument GetInnerDocument()
            {
                return dummyDocument;
            }
        }

        private class DummyRepository : EntityRepository<DummyEntity, DummyDocument>
        {
            public DummyRepository(IDocumentSession session) : base(session)
            {
            }

            protected override DummyEntity Create(DummyDocument doc)
            {
                return new DummyEntity(doc);
            }
        }
    }

}
