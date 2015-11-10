
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordReset : ILiteEvent
    {
        public string Password { set; get; }
        public string PasswordSalt { set; get; }
    }
}
