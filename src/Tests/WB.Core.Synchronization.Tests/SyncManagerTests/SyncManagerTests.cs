using System;
using Moq;
using NUnit.Framework;
using WB.Core.Synchronization.Implementation.SyncManager;
using WB.Core.Synchronization.SyncProvider;

namespace WB.Core.Synchronization.Tests.SyncManagerTests
{
    [TestFixture]
    class SyncManagerTests
    {
        /*[Test]*/
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
            var syncProvider = new Mock<ISyncProvider>();

            ISyncManager manager = CreateDefaultSyncManager(syncProvider.Object);

            //Act
            var result = manager.ItitSync(clientIdentifier);

            //Assert
            syncProvider.Verify(x => x.CheckAndCreateNewSyncActivity(It.IsAny<ClientIdentifier>()), Times.Once());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ClientInstanceKey, Is.EqualTo(clientIdentifier.ClientInstanceKey));

        }


        private SyncManager CreateDefaultSyncManager(ISyncProvider syncProvider)
        {
            return new SyncManager(syncProvider);
        }
    }
}
