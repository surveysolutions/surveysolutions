using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountUpdated : ILiteEvent
    {
        public string UserName { get; set; }
        public string PasswordQuestion { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    }
}
