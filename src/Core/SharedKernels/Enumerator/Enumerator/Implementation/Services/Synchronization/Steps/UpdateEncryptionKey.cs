using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class UpdateEncryptionKey : SynchronizationStep
    {
        private readonly ISecureStorage secureStorage;

        public UpdateEncryptionKey(ISynchronizationService synchronizationService, ISecureStorage secureStorage, ILogger logger, 
            int sortOrder) : base(sortOrder, synchronizationService, logger)
        {
            this.secureStorage = secureStorage;
        }

        public override async Task ExecuteAsync()
        {
            if (this.secureStorage.Contains(RsaEncryptionService.PublicKey)) return;

            var publicKeyForEncryption = await this.synchronizationService.GetPublicKeyForEncryptionAsync(CancellationToken.None);
            if (!string.IsNullOrEmpty(publicKeyForEncryption))
                this.secureStorage.Store(RsaEncryptionService.PublicKey, Convert.FromBase64String(publicKeyForEncryption));
        }
    }
}
