using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.Tests.SyncManagerTests
{
    [TestFixture]
    class SyncManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void ItitSync_when_New_ClientIdentifier_Arrived_New_Device_Is_Stored()
        {
            //Arrange
            var clientIdentifier = new ClientIdentifier()
                {
                    ClientDeviceKey = "device1",
                    ClientRegistrationKey = null,
                    ClientVersionIdentifier = "v1",
                    ClientInstanceKey = Guid.NewGuid(),
                    CurrentProcessKey = null,
                    LastSyncKey = null
                };

            Guid deviceId = Guid.NewGuid();
            HandshakePackage package = new HandshakePackage() { ClientInstanceKey = clientIdentifier.ClientInstanceKey, ClientRegistrationKey = deviceId };

            var syncProvider = new Mock<ISyncProvider>();
            syncProvider.Setup(d => d.CheckAndCreateNewSyncActivity(clientIdentifier)).Returns(package);


            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            var result = manager.ItitSync(clientIdentifier);

            //Assert
            syncProvider.Verify(x => x.CheckAndCreateNewSyncActivity(It.IsAny<ClientIdentifier>()), Times.Once());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ClientInstanceKey, Is.EqualTo(clientIdentifier.ClientInstanceKey));
            Assert.That(result.ClientRegistrationKey, Is.EqualTo(deviceId));
        }

        [Test]
        public void ReceiveSyncPackage_when_Valid_Request_Arrived_SyncPackage_Is_returned()
        {
            //Arrange
            var syncProvider = new Mock<ISyncProvider>();
            Guid registrationId = Guid.NewGuid();
            Guid itemId = Guid.NewGuid();
            long sequence = 1;

            SyncItem expectedResult = new SyncItem() { Id = itemId };
            syncProvider.Setup(d => d.GetSyncItem(registrationId, itemId, sequence)).Returns(expectedResult);


            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            var result = manager.ReceiveSyncPackage(registrationId, itemId,sequence);

            //Assert
            syncProvider.Verify(x => x.GetSyncItem(registrationId, itemId, sequence), Times.Once());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ItemsContainer.Count, Is.EqualTo(1));
            Assert.That(result.IsErrorOccured, Is.EqualTo(false));
            Assert.That(result.ItemsContainer[0].Id, Is.EqualTo(itemId));
        }

        [Test]
        public void ReceiveSyncPackage_when_Valid_Request_Arrived_but_Item_was_NOT_Found_SyncPackage_with_error_status_Is_returned()
        {
            //Arrange
            var syncProvider = new Mock<ISyncProvider>();
            Guid registrationId = Guid.NewGuid();
            Guid itemId = Guid.NewGuid();
            long sequence = 1;

            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            var result = manager.ReceiveSyncPackage(registrationId, itemId, sequence);

            //Assert
            syncProvider.Verify(x => x.GetSyncItem(registrationId, itemId, sequence), Times.Once());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsErrorOccured, Is.EqualTo(true));
        }


        [Test]
        public void ItitSync_when_New_ClientIdentifier_with_empty_ClientInstanceKey_Arrived_Exeption_is_thrown()
        {
            //Arrange
            var clientIdentifier = new ClientIdentifier()
            {
                ClientDeviceKey = "device1",
                ClientRegistrationKey = null,
                ClientVersionIdentifier = "v1",
                ClientInstanceKey = Guid.Empty,
                CurrentProcessKey = null,
                LastSyncKey = null
            };
           
            var syncProvider = new Mock<ISyncProvider>();
            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            TestDelegate act = () => manager.ItitSync(clientIdentifier);
            
            //Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ItitSync_when_New_ClientIdentifier_with_empty_DeviceKey_Arrived_Exeption_is_thrown()
        {
            //Arrange
            var clientIdentifier = new ClientIdentifier()
            {
                ClientDeviceKey = "",
                ClientRegistrationKey = null,
                ClientVersionIdentifier = "v1",
                ClientInstanceKey = Guid.NewGuid(),
                CurrentProcessKey = null,
                LastSyncKey = null
            };

            var syncProvider = new Mock<ISyncProvider>();
            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            TestDelegate act = () => manager.ItitSync(clientIdentifier);

            //Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Test]
        public void ItitSync_when_New_ClientIdentifier_with_empty_Version_Arrived_Exeption_is_thrown()
        {
            //Arrange
            var clientIdentifier = new ClientIdentifier()
            {
                ClientDeviceKey = "device1",
                ClientRegistrationKey = null,
                ClientVersionIdentifier = "",
                ClientInstanceKey = Guid.NewGuid(),
                CurrentProcessKey = null,
                LastSyncKey = null
            };

            var syncProvider = new Mock<ISyncProvider>();
            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            TestDelegate act = () => manager.ItitSync(clientIdentifier);

            //Assert
            Assert.Throws<ArgumentException>(act);
        }
        
        private SyncManager CreateDefaultSyncManager(ISyncProvider syncProvider)
        {
            return new SyncManager(syncProvider);
        }
    }
}
