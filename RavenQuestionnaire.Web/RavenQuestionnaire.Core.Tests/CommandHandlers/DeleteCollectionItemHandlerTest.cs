using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Collection;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteCollectionItemHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_CollectionItemIsDeletedFromRepository()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Id = "cID";
            Collection entity = new Collection(innerDocument);
            var collectionItem = new CollectionItem(Guid.Empty, "key", "value");
            innerDocument.Items.Add(collectionItem);
            Assert.True(innerDocument.Items.Count==1);
            Mock<ICollectionRepository> collectionRepositoryMock=new Mock<ICollectionRepository>();
            collectionRepositoryMock.Setup(x => x.Load("collectiondocuments/cID")).Returns(entity);
            DeleteCollectionItemHandler handler=new DeleteCollectionItemHandler(collectionRepositoryMock.Object);
            handler.Handle(new DeleteCollectionItemCommand(entity.CollectionId, null, Guid.Empty));
            Assert.True(innerDocument.Items.Count==0);
        }
    }
}
