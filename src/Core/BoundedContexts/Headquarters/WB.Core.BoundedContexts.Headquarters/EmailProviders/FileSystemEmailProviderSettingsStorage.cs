using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class FileSystemEmailProviderSettingsStorage : IPlainKeyValueStorage<EmailProviderSettings>
    {
        private readonly FileSystemEmailServiceSettings settings;

        public FileSystemEmailProviderSettingsStorage(FileSystemEmailServiceSettings settings)
        {
            this.settings = settings;
        }

        public EmailProviderSettings GetById(string id)
        {
            return new EmailProviderSettings()
            {
                Provider = (EmailProvider)777,
                Address = settings.Address,
                SenderAddress = settings.SenderAddress,
                SenderName = settings.SenderName,
                ReplyAddress = settings.ReplyAddress,
            };
        }

        public bool HasNotEmptyValue(string id)
        {
            throw new NotSupportedException();
        }

        public void Remove(string id)
        {
            throw new NotSupportedException();
        }

        public void Store(EmailProviderSettings entity, string id)
        {
            throw new NotSupportedException();
        }
    }
}
