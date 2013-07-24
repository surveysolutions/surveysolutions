using System;
using System.Collections.Generic;
using Main.Core.Commands.Sync;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.Tests.SyncProviderTests
{
    using NUnit.Framework;
    using SyncProvider;

    [TestFixture]
    public class SyncProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            var commandService = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(commandService.Object);
        }

        [Test]
        public void CheckAndCreateNewSyncActivity_when_New_Valid_ClientIdentifier_Arrived_then_Device_Is_Stored()
        {
            //Arrange
            var commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(commandService.Object);

            ISyncProvider provider = CreateDefaultSyncProvider();
            var clientIdentifier = new ClientIdentifier()
                {
                    ClientDeviceKey = "device1",
                    ClientInstanceKey = Guid.NewGuid(),
                    ClientRegistrationKey = null,
                    ClientVersionIdentifier = "v1",
                    CurrentProcessKey = null,
                    LastSyncKey = null
                };

            //Act
            var item = provider.CheckAndCreateNewSyncActivity(clientIdentifier);

            //Assert
            commandService.Verify(x => x.Execute(It.IsAny<CreateClientDeviceCommand>()),Times.Once());
            Assert.That(item.ClientInstanceKey, Is.EqualTo(clientIdentifier.ClientInstanceKey));
        }

        [Test]
        public void HandleSyncItem_If_Item_Content_Is_Empty_Exception_is_thrown()
        {
            //Arrange
            var commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(commandService.Object);
            ISyncProvider provider = CreateDefaultSyncProvider();

            Guid syncId = Guid.NewGuid();

            SyncItem item = new SyncItem() {Content = string.Empty};

            //Act
            TestDelegate act = () => provider.HandleSyncItem(item, syncId);

            //Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void HandleSyncItem_If_Item_Is_Null_Exception_is_thrown()
        {
            //Arrange
            var commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(commandService.Object);
            ISyncProvider provider = CreateDefaultSyncProvider();

            Guid syncId = Guid.NewGuid();

            SyncItem item = null;

            //Act
            TestDelegate act = () => provider.HandleSyncItem(item, syncId);

            //Assert
            var ex = Assert.Throws<ArgumentException>(act);
            //Assert.That(ex.ParamName, Is.EqualTo("Sync Item is not set."));
        }

        /*[Test]*/
        public void GetSyncItem_when_Valid_SyncItem_Arrived_()
        {
            //Arrange
            var commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(commandService.Object);
            var registrationKey = Guid.NewGuid();
            var itemKey = Guid.NewGuid();
            long sequence = 1;

            var clientDevice = new ClientDeviceDocument()
                {
                    ClientInstanceKey = Guid.NewGuid(),
                    PublicKey = registrationKey,
                    LastSyncItemIdentifier = 2
                };

            var devices = new Mock<IQueryableReadSideRepositoryReader<ClientDeviceDocument>>();
            devices.Setup(d => d.GetById(clientDevice.PublicKey)).Returns(clientDevice);

            var syncStoredItem = new SyncItem()
                {
                    Id = itemKey,
                    ChangeTracker = 5
                };

            var storage = new Mock<ISynchronizationDataStorage>();
            storage.Setup(s => s.GetLatestVersion(itemKey)).Returns(syncStoredItem);
            var incomeStorage = new Mock<IIncomePackagesRepository>();
            var logger = new Mock<ILogger>();

            var provider = CreateDefaultSyncProvider(devices.Object, storage.Object,incomeStorage.Object, logger.Object);

            //Act
            var syncItem = provider.GetSyncItem(registrationKey, itemKey, sequence);

            //Assert
            commandService.Verify(x => x.Execute(It.IsAny<UpdateClientDeviceLastSyncItemCommand>()),Times.Once());
            Assert.That(syncItem,Is.Not.Null);
            Assert.That(syncItem.Id, Is.EqualTo(itemKey));
        }

      
        public void GetSyncItem_when_ClientId_and_sequence_provided_SyncItem_Is_returned()
        {

        }

        [Test]
        public void GetAllARIds_when_ClientId_and_DeviceRegistrationKey_Provided_ListARIsReturned()
        {
            //Arrange
            var commandService = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault(commandService.Object);

            var registrationKey = Guid.NewGuid();
            var clientDevice = new ClientDeviceDocument()
            {
                ClientInstanceKey = Guid.NewGuid(),
                PublicKey = registrationKey,
                LastSyncItemIdentifier = 2
            };

            var devices = new Mock<IQueryableReadSideRepositoryReader<ClientDeviceDocument>>();
            devices.Setup(d => d.GetById(clientDevice.PublicKey)).Returns(clientDevice);

            var userId = Guid.NewGuid();

            var listIds = new List<Guid>() {Guid.NewGuid(),Guid.NewGuid()};

            var storage = new Mock<ISynchronizationDataStorage>();
            storage.Setup(s => s.GetChunksCreatedAfter(2, userId)).Returns(listIds);
            var incomeStorage = new Mock<IIncomePackagesRepository>();
            var logger = new Mock<ILogger>();

            var provider = CreateDefaultSyncProvider(devices.Object, storage.Object, incomeStorage.Object, logger.Object);

            //Act
            var ids = provider.GetAllARIds(userId, registrationKey);

            //Assert
            Assert.That(ids, Is.EqualTo(listIds));

        }

        private SyncProvider CreateDefaultSyncProvider()
        {
            var devices = new Mock<IQueryableReadSideRepositoryReader<ClientDeviceDocument>>();
            var storage = new Mock<ISynchronizationDataStorage>();
            var incomeStorage = new Mock<IIncomePackagesRepository>();
            var logger = new Mock<ILogger>();

            return CreateDefaultSyncProvider(devices.Object, storage.Object, incomeStorage.Object, logger.Object);
        }
        private SyncProvider CreateDefaultSyncProvider(IQueryableReadSideRepositoryReader<ClientDeviceDocument> devices, ISynchronizationDataStorage storage, IIncomePackagesRepository incomeRepository, ILogger logger)
        {
            return new SyncProvider(devices, storage, incomeRepository, logger);
        }

    }
}
