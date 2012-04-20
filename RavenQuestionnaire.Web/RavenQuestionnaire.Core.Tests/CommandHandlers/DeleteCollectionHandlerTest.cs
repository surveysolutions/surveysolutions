using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.CommandHandlers.Collection;



namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteCollectionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_CollectionIsDeletedFromRepository()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Id = "qID";
            Collection entity = new Collection(innerDocument);
            Mock<ICollectionRepository> collectionRepositoryMock = new Mock<ICollectionRepository>();
            collectionRepositoryMock.Setup(x => x.Load("collectiondocuments/qID")).Returns(entity);
            DeleteCollectionHandler handler=new DeleteCollectionHandler(collectionRepositoryMock.Object);
            handler.Handle(new DeleteCollectionCommand(entity.CollectionId, null));
            collectionRepositoryMock.Verify(x => x.Remove(entity));
        }
    }
}
