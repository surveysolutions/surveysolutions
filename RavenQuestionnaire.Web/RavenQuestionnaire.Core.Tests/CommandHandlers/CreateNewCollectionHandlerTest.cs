using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.CommandHandlers.Collection;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewCollectionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewCollectionIsAddedToRepository()
        {
            Mock<ICollectionRepository> collectionRepositoryMock = new Mock<ICollectionRepository>();
            CreateNewCollectionHandler  handler=new CreateNewCollectionHandler(collectionRepositoryMock.Object);
            handler.Handle(new CreateNewCollectionCommand("CollectionItems", new List<CollectionItem>()));
            collectionRepositoryMock.Verify(x=>x.Add(It.IsAny<Collection>()), Times.Once());
        }
    }
}
