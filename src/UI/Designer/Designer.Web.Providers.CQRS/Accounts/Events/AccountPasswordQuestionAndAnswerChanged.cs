
namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordQuestionAndAnswerChanged
    {
        public string PasswordQuestion { set; get; }
        public string PasswordAnswer { set; get; }
    }
}
