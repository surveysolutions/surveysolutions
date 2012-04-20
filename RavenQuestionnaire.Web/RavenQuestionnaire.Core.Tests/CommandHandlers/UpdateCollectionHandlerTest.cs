using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.CommandHandlers.Collection;


namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateCollectionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_CollectionIsUpdatedToRepository()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Id = "cId";
            var entity = new Collection(innerDocument);
            Mock<ICollectionRepository> collectionRepositoryMock = new Mock<ICollectionRepository>();
            collectionRepositoryMock.Setup(x => x.Load("collectiondocuments/cId")).Returns(entity);
            UpdateCollectionHandler handler=new UpdateCollectionHandler(collectionRepositoryMock.Object);
            handler.Handle(new UpdateCollectionCommand(entity.CollectionId, "collection", new List<CollectionItem>()));
            Assert.True(innerDocument.Name=="collection");
        }
    }
}