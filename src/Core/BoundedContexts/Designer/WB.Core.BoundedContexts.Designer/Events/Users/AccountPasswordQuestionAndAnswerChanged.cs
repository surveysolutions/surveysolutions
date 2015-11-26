
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordQuestionAndAnswerChanged : ILiteEvent
    {
        public string PasswordQuestion { set; get; }
        public string PasswordAnswer { set; get; }
    }
}
