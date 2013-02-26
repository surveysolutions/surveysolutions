namespace Designer.Web.Providers.CQRS
{
    public class AccountCreated
    {
        public object ProviderUserKey { get; set; }
        public string ApplicationName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}