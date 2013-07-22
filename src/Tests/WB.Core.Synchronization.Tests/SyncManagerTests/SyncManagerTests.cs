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


        private SyncManager CreateDefaultSyncManager(ISyncProvider syncProvider)
        {
            return new SyncManager(syncProvider);
        }
    }
}
