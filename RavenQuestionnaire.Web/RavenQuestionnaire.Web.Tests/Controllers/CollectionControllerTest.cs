using Moq;
using System;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Web.Controllers;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Collection;
using RavenQuestionnaire.Core.Commands.Collection;
using RavenQuestionnaire.Core.Entities.SubEntities;


namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class CollectionControllerTest
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public CollectionController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            CommandServiceMock=new Mock<ICommandService>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            NcqrsEnvironment.SetDefault<ICommandService>(CommandServiceMock.Object);
            Controller = new CollectionController(CommandInvokerMock.Object, ViewRepositoryMock.Object);
        }

        [Test]
        public void WhenNewCollectionAddIsSubmittedWIthValidModel_CommandIsSent()
        {
            var innerDocument = new CollectionDocument();
            innerDocument.Id = string.Empty;
            innerDocument.Name = "CollectionName";
            var entity = new Collection(innerDocument);
            var items = new List<CollectionItem>();
            items.Add(new CollectionItem(Guid.NewGuid(), "key", "value"));
            entity.AddCollectionItems(items);
            var collectionView = new CollectionView(innerDocument);
            ViewRepositoryMock.Setup(
                x =>
                x.Load<CollectionViewInputModel, CollectionView>(
                    It.Is<CollectionViewInputModel>(v => v.CollectionId.Equals("collectiondocuments/")))).
                Returns(collectionView);
            Controller.Edit(collectionView);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewCollectionCommand>()), Times.Once());
        }

        [Test]
        public void WhenCollectionUpdateIsSubmittedWIthValidModel_CommandIsSent()
        {
            var innerDocument = new CollectionDocument();
            innerDocument.Id = "collectionID";
            innerDocument.Name = "CollectionName";
            var entity = new Collection(innerDocument);
            var items = new List<CollectionItem>();
            items.Add(new CollectionItem(Guid.NewGuid(), "key", "value"));
            entity.AddCollectionItems(items);
            var collectionView = new CollectionView(innerDocument);
            ViewRepositoryMock.Setup(
                x =>
                x.Load<CollectionViewInputModel, CollectionView>(
                    It.Is<CollectionViewInputModel>(v => v.CollectionId.Equals("collectiondocuments/collectionID")))).
                Returns(collectionView);
            Controller.Edit(collectionView);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<UpdateCollectionCommand>()), Times.Once());
        }

        [Test]
        public void When_DeleteCollectionItemIsExecuted()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Id = "collectionId";
            innerDocument.Items.Add(new CollectionItem(Guid.Empty, "key", "value"));
            Collection entity = new Collection(innerDocument);
            entity.DeleteItemFromCollection(Guid.Empty);
            Mock<ICollectionRepository> collectionRepositoryMock=new Mock<ICollectionRepository>();
            collectionRepositoryMock.Setup(x => x.Load("collectiondocuments/collectionId")).Returns(entity);
            Controller.DeleteItem(entity.CollectionId, Guid.Empty);
            CommandInvokerMock.Verify(x=>x.Execute(It.IsAny<DeleteCollectionItemCommand>()), Times.Once());
        }

        [Test]
        public void When_DeleteCollectionIsExecuted()
        {
            CollectionDocument innerDocument = new CollectionDocument();
            innerDocument.Id = "collectionId";
            Collection entity = new Collection(innerDocument);
            Mock<ICollectionRepository> collectionRepositoryMock = new Mock<ICollectionRepository>();
            collectionRepositoryMock.Setup(x => x.Load("collectiondocuments/collectionId")).Returns(entity);
            Controller.Delete(entity.CollectionId);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteCollectionCommand>()), Times.Once());
        }

    }
}
