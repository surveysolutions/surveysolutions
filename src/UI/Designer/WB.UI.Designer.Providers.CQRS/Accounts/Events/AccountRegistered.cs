namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    public class AccountRegistered
    {
        public string ApplicationName { set; get; }
        public string UserName { set; get; }
        public string Email { set; get; }
        public object ProviderUserKey { set; get; }
        public string ConfirmationToken { set; get; }

        public System.DateTime CreatedDate { get; set; }
    }
}
