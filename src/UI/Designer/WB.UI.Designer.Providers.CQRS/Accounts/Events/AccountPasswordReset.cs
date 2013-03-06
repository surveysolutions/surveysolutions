
namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountPasswordReset
    {
        public string Password { set; get; }
        public string PasswordSalt { set; get; }
    }
}
