using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services;

public class GlobalNoticeService : IGlobalNoticeService
{
    public const int MaxMessageLength = 1000;

    private readonly IPlainKeyValueStorage<GlobalNotice> storage;

    public GlobalNoticeService(IPlainKeyValueStorage<GlobalNotice> storage)
    {
        this.storage = storage;
    }

    public string? GetMessage()
    {
        var message = this.storage.GetById(AppSetting.GlobalNoticeKey)?.Message;
        return string.IsNullOrWhiteSpace(message) ? null : message;
    }

    public void SetMessage(string? message)
    {
        var trimmed = message?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            this.storage.Remove(AppSetting.GlobalNoticeKey);
            return;
        }

        if (trimmed.Length > MaxMessageLength)
            trimmed = trimmed.Substring(0, MaxMessageLength);

        this.storage.Store(new GlobalNotice { Message = trimmed }, AppSetting.GlobalNoticeKey);
    }
}
