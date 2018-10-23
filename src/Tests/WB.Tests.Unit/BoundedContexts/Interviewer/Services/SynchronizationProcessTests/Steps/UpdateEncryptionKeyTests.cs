using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(UpdateEncryptionKey))]
    internal class UpdateEncryptionKeyTests
    {
        [Test]
        public async Task when_secure_storage_has_public_rsa_key_from_hq()
        {
            // arrange
            var mockOfSecureStorage = new Mock<ISecureStorage>();
            mockOfSecureStorage.Setup(x => x.Contains(RsaEncryptionService.PublicKey)).Returns(true);
            var mockOfSynchronizationService = new Mock<ISynchronizationService>();

            var updateEncryptionKeyStep = CreateUpdateEncryptionKey(
                secureStorage: mockOfSecureStorage.Object,
                synchronizationService: mockOfSynchronizationService.Object);
            // act
            await updateEncryptionKeyStep.ExecuteAsync();
            // assert
            mockOfSynchronizationService.Verify(x => x.GetPublicKeyForEncryptionAsync(It.IsAny<CancellationToken>()), Times.Never);
            mockOfSecureStorage.Verify(x => x.Store(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
        }

        [Test]
        public async Task when_enumerator_dont_have_public_rsa_key_from_hq()
        {
            // arrange
            var mockOfSecureStorage = new Mock<ISecureStorage>();
            mockOfSecureStorage.Setup(x => x.Contains(RsaEncryptionService.PublicKey)).Returns(false);
            var mockOfSynchronizationService = new Mock<ISynchronizationService>();
            mockOfSynchronizationService.Setup(x => x.GetPublicKeyForEncryptionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Convert.ToBase64String(Encoding.UTF8.GetBytes("rsa public key from hq"))));

            var updateEncryptionKeyStep = CreateUpdateEncryptionKey(
                secureStorage: mockOfSecureStorage.Object,
                synchronizationService: mockOfSynchronizationService.Object);
            // act
            await updateEncryptionKeyStep.ExecuteAsync();
            // assert
            mockOfSynchronizationService.Verify(x => x.GetPublicKeyForEncryptionAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockOfSecureStorage.Verify(x => x.Store(RsaEncryptionService.PublicKey, It.IsAny<byte[]>()), Times.Once);
        }

        private UpdateEncryptionKey CreateUpdateEncryptionKey(
            ISynchronizationService synchronizationService = null, 
            ISecureStorage secureStorage = null) => new UpdateEncryptionKey(synchronizationService, secureStorage, Mock.Of<ILogger>(), 1);
    }
}
