using System;
using System.Collections.Generic;
using System.Text;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Headquarters.Implementation.Services
{
    internal class SecureStorage : ISecureStorage
    {
        private readonly IPlainKeyValueStorage<RsaEncryptionSettings> appSettingsStorage;

        public SecureStorage(IPlainKeyValueStorage<RsaEncryptionSettings> appSettingsStorage)
        {
            this.appSettingsStorage = appSettingsStorage;
        }

        public void Store(string key, byte[] dataBytes) => throw new NotImplementedException();

        public byte[] Retrieve(string key)
        {
            if (key == RsaEncryptionService.PublicKey)
                return Encoding.UTF8.GetBytes(this.appSettingsStorage.GetById(AppSetting.RsaKeysForEncryption)
                    ?.PublicKey);
            if(key == RsaEncryptionService.PrivateKey)
                return Encoding.UTF8.GetBytes(this.appSettingsStorage.GetById(AppSetting.RsaKeysForEncryption)
                    ?.PrivateKey);

            throw new KeyNotFoundException(key);
        }

        public void Delete(string key) => throw new NotImplementedException();

        public bool Contains(string key) => key == RsaEncryptionService.PrivateKey || key == RsaEncryptionService.PrivateKey;
    }
}
